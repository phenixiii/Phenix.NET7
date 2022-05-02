using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Mapper.Schema;
using Phenix.Core.Reflection;

namespace Phenix.Business
{
    /// <summary>
    /// 可撤销编辑的实体基类
    /// </summary>
    [Serializable]
    public abstract class UndoableBase<T> : EntityBase<T>, ISupportUndo
        where T : UndoableBase<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected UndoableBase()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected UndoableBase(string dataSourceKey, bool? isNew, bool? isSelfDeleted, bool? isSelfDirty,
            IDictionary<string, object> oldPropertyValues, IDictionary<string, bool?> dirtyPropertyNames)
            : base(dataSourceKey)
        {
            if (isNew.HasValue)
                _executeAction = _executeAction | ExecuteAction.Insert;
            if (isSelfDeleted.HasValue)
                _executeAction = _executeAction | ExecuteAction.Delete;
            if (isSelfDirty.HasValue)
                _executeAction = _executeAction | ExecuteAction.Update;
            _oldPropertyValues = oldPropertyValues;
            _dirtyPropertyNames = dirtyPropertyNames;
        }

        #region 工厂

        /// <summary>
        /// 新增实体对象(自动填充主键和保留字段)
        /// </summary>
        /// <param name="source">数据源</param>
        public static T New(T source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            T result = (T) source.MemberwiseClone();
            result.InitializeSelf();
            return result;
        }

        #endregion

        #region 属性

        private ExecuteAction _executeAction;

        /// <summary>
        /// 离线状态
        /// </summary>
        public bool IsFetched
        {
            get { return _executeAction == ExecuteAction.None; }
        }

        /// <summary>
        /// 新增状态
        /// </summary>
        public bool IsNew
        {
            get { return (_executeAction & ExecuteAction.Insert) == ExecuteAction.Insert; }
            set
            {
                if (value && !IsNew)
                {
                    _executeAction = _executeAction | ExecuteAction.Insert;

                    FillReservedFields(ExecuteAction.Insert);
                }
                else if (!value && IsNew)
                    _executeAction = _executeAction & (ExecuteAction.Update | ExecuteAction.Delete);
            }
        }

        /// <summary>
        /// 删除状态
        /// </summary>
        public bool IsSelfDeleted
        {
            get { return (_executeAction & ExecuteAction.Delete) == ExecuteAction.Delete; }
            set
            {
                if (value && !IsSelfDeleted)
                    _executeAction = _executeAction | ExecuteAction.Delete;
                else if (!value && IsSelfDeleted)
                    _executeAction = _executeAction & (ExecuteAction.Insert | ExecuteAction.Update);
            }
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        public bool IsSelfDirty
        {
            get { return (_executeAction & ExecuteAction.Update) == ExecuteAction.Update; }
            set
            {
                if (value && !IsSelfDirty)
                {
                    Dictionary<string, object> oldPropertyValues = new Dictionary<string, object>(StringComparer.Ordinal);
                    foreach (KeyValuePair<string, Property> kvp in SelfSheet.GetProperties(this.GetType(), TargetTable, false))
                        oldPropertyValues.Add(kvp.Key, kvp.Value.Field != null ? kvp.Value.Field.GetValue(this) : kvp.Value.GetValue(this));
                    _oldPropertyValues = oldPropertyValues;
                    _dirtyPropertyNames = null;
                    _executeAction = _executeAction | ExecuteAction.Update;

                    FillReservedFields(ExecuteAction.Update);
                }
                else if (!value && IsSelfDirty)
                {
                    IDictionary<string, object> oldPropertyValues = _oldPropertyValues;
                    _oldPropertyValues = null;
                    _dirtyPropertyNames = null;
                    _executeAction = _executeAction & (ExecuteAction.Insert | ExecuteAction.Delete);
                    if (oldPropertyValues != null)
                        foreach (KeyValuePair<string, object> kvp in oldPropertyValues)
                        {
                            Property property = SelfSheet.GetProperty(this.GetType(), kvp.Key);
                            if (property.Field != null)
                                property.Field.Set(this, kvp.Value);
                            else
                                property.Set(this, kvp.Value);
                        }
                }
            }
        }

        private IDictionary<string, object> _oldPropertyValues;

        /// <summary>
        /// 旧属性值
        /// </summary>
        public IDictionary<string, object> OldPropertyValues
        {
            get { return _oldPropertyValues; }
        }

        private IDictionary<string, bool?> _dirtyPropertyNames;

        /// <summary>
        /// 脏属性名
        /// </summary>
        public IDictionary<string, bool?> DirtyPropertyNames
        {
            get { return _dirtyPropertyNames ?? (_dirtyPropertyNames = new Dictionary<string, bool?>(StringComparer.Ordinal)); }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化自己
        /// </summary>
        protected override void InitializeSelf()
        {
            IsNew = true;
        }

        /// <summary>
        /// MarkFetched
        /// </summary>
        protected void MarkFetched()
        {
            _executeAction = ExecuteAction.None;
            _oldPropertyValues = null;
            _dirtyPropertyNames = null;
        }

        #region 编辑状态

        /// <summary>
        /// 启动编辑(仅允许编辑非IsSelfDirty状态的对象)
        /// 快照当前数据
        /// </summary>
        public virtual void BeginEdit()
        {
            if (IsSelfDirty)
                throw new System.ComponentModel.DataAnnotations.ValidationException("禁止编辑已处于编辑状态的对象");

            IsSelfDirty = true;
        }

        /// <summary>
        /// 撤销编辑(仅允许撤销IsSelfDirty状态的对象)
        /// 恢复快照数据
        /// </summary>
        public virtual void CancelEdit()
        {
            if (!IsSelfDirty)
                throw new System.ComponentModel.DataAnnotations.ValidationException("仅允许回滚已处于编辑状态的对象");

            IsSelfDirty = false;
        }

        /// <summary>
        /// 应用编辑
        /// 丢弃快照数据
        /// </summary>
        public virtual void ApplyEdit()
        {
            if (!IsSelfDeleted)
                MarkFetched();
        }

        #endregion

        #region DirtyValues

        /// <summary>
        /// 获取旧值
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 ArgumentException; 如果为 false, 则在找不到信息时返回 null</param>
        public object GetOldValue(Expression<Func<T, object>> propertyLambda, bool throwIfNotFound = true)
        {
            if (propertyLambda == null)
                throw new ArgumentNullException(nameof(propertyLambda));
            if (!IsSelfDirty)
                throw new System.ComponentModel.DataAnnotations.ValidationException("仅允许查询编辑状态的对象旧值");

            PropertyInfo propertyInfo = Utilities.GetPropertyInfo(propertyLambda, throwIfNotFound);
            if (propertyInfo != null && _oldPropertyValues != null && _oldPropertyValues.TryGetValue(propertyInfo.Name, out object result))
                return Utilities.ChangeType(result, propertyInfo.PropertyType);

            if (throwIfNotFound)
                throw new InvalidOperationException(String.Format("{0} 应该是类 {1} 某个映射表字段且不是水印字段的属性表达式", propertyLambda.Name, typeof(T).FullName));
            return null;
        }

        /// <summary>
        /// 是否脏属性(null代表仅基于新旧值的判断)
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        public bool? IsDirtyProperty(Expression<Func<T, object>> propertyLambda)
        {
            return DirtyPropertyNames.TryGetValue(Utilities.GetPropertyInfo(propertyLambda).Name, out bool? result) ? result : null;
        }

        /// <summary>
        /// 设置脏属性
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="isDirty">是否脏数据(null代表仅基于新旧值的判断)</param>
        public void SetDirtyProperty(Expression<Func<T, object>> propertyLambda, bool? isDirty = true)
        {
            IsSelfDirty = true;
            DirtyPropertyNames[Utilities.GetPropertyInfo(propertyLambda).Name] = isDirty;
        }

        /// <summary>
        /// 设置脏属性值
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="newValue">新属性值</param>
        public void SetDirtyValue(Expression<Func<T, object>> propertyLambda, object newValue)
        {
            IsSelfDirty = true;
            Property property = SelfSheet.GetProperty(propertyLambda);
            if (property.Field != null)
                property.Field.Set(this, newValue);
            else
                property.Set(this, newValue);
            DirtyPropertyNames[property.PropertyInfo.Name] = true;
        }

        /// <summary>
        /// 设置脏属性值
        /// </summary>
        /// <param name="source">数据源</param>
        public bool SetDirtyValues(T source)
        {
            if (IsSelfDirty)
                throw new System.ComponentModel.DataAnnotations.ValidationException("禁止导入已处于编辑状态的对象");

            bool result = false;
            Dictionary<string, object> oldPropertyValues = new Dictionary<string, object>(StringComparer.Ordinal);
            Dictionary<string, bool?> dirtyPropertyNames = new Dictionary<string, bool?>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, Property> kvp in SelfSheet.GetProperties(this.GetType(), TargetTable, false))
            {
                object value = kvp.Value.Field != null ? kvp.Value.Field.GetValue(this) : kvp.Value.GetValue(this);
                oldPropertyValues.Add(kvp.Key, value);
                object sourceValue = kvp.Value.Field != null ? kvp.Value.Field.GetValue(source) : kvp.Value.GetValue(source);
                if (!object.Equals(value, sourceValue))
                {
                    result = true;
                    if (kvp.Value.Field != null)
                        kvp.Value.Field.Set(this, sourceValue);
                    else
                        kvp.Value.Set(this, sourceValue);
                    dirtyPropertyNames.Add(kvp.Key, true);
                }
            }

            if (result)
            {
                _oldPropertyValues = oldPropertyValues;
                _dirtyPropertyNames = dirtyPropertyNames;
                _executeAction = _executeAction | ExecuteAction.Update;

                FillReservedFields(ExecuteAction.Update);
            }

            return result;
        }

        /// <summary>
        /// 提取脏属性值
        /// </summary>
        protected IDictionary<string, object> GetDirtValues()
        {
            if (_oldPropertyValues == null)
                return null;

            Dictionary<string, object> result = new Dictionary<string, object>(_oldPropertyValues.Count);
            foreach (KeyValuePair<string, Property> kvp in SelfSheet.GetProperties(this.GetType(), TargetTable, false))
                if (_oldPropertyValues.TryGetValue(kvp.Key, out object oldValue))
                    if (DirtyPropertyNames.TryGetValue(kvp.Key, out bool? isDirty) && isDirty.HasValue)
                    {
                        if (isDirty.Value)
                        {
                            object value = kvp.Value.Field != null ? kvp.Value.Field.GetValue(this) : kvp.Value.GetValue(this);
                            result.Add(kvp.Key, value);
                        }
                    }
                    else
                    {
                        object value = kvp.Value.Field != null ? kvp.Value.Field.GetValue(this) : kvp.Value.GetValue(this);
                        if (!object.Equals(oldValue, value))
                            result.Add(kvp.Key, value);
                    }

            return result;
        }

        #endregion

        #region SaveSelf

        /// <summary>
        /// 保存自己
        /// </summary>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        public int SaveSelf(bool checkTimestamp = true)
        {
            return Database.ExecuteGet((Func<DbTransaction, bool, int>) SaveSelf, checkTimestamp);
        }

        /// <summary>
        /// 保存自己
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        public int SaveSelf(DbConnection connection, bool checkTimestamp = true)
        {
            return DbConnectionHelper.ExecuteGet(connection, SaveSelf, checkTimestamp);
        }

        /// <summary>
        /// 保存自己
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <param name="checkTimestamp">是否检查时间戳（不一致时抛出Phenix.Core.Data.Rule.OutdatedDataException）</param>
        public virtual int SaveSelf(DbTransaction transaction, bool checkTimestamp = true)
        {
            int result = 0;

            if (IsNew && !IsSelfDeleted)
                result = InsertSelf(transaction);
            else if (IsSelfDeleted && !IsNew)
                result = DeleteSelf(transaction);
            else if (IsSelfDirty && !IsSelfDeleted)
                result = UpdateSelf(transaction, GetDirtValues(), checkTimestamp);
            ApplyEdit();

            return result;
        }

        #endregion

        #endregion
    }
}
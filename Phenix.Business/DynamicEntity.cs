using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

namespace Phenix.Business
{
    /// <summary>
    /// 动态实体
    /// </summary>
    [Serializable]
    public class DynamicEntity : DynamicObject
    {
        [Newtonsoft.Json.JsonConstructor]
        private DynamicEntity(IDictionary<string, object> propertyValues)
        {
            _propertyValues = propertyValues ?? new Dictionary<string, object>(0);
        }

        private DynamicEntity(IDictionary<string, object> propertyValues, Sheet selfSheet)
            : this(propertyValues)
        {
            _selfSheet = selfSheet;
        }

        #region 工厂

        /// <summary>
        /// 构建动态实体
        /// </summary>
        /// <param name="selfSheet">操作单子</param>
        public static DynamicEntity Fetch(Sheet selfSheet)
        {
            return Fetch(null, selfSheet);
        }

        /// <summary>
        /// 构建动态实体
        /// </summary>
        /// <param name="nameValues">"属性名-属性值"键值队列</param>
        public static DynamicEntity Fetch(params NameValue[] nameValues)
        {
            if (nameValues != null)
            {
                Dictionary<string, object> propertyValues = new Dictionary<string, object>(nameValues.Length);
                foreach (NameValue item in nameValues)
                    propertyValues.Add(item.Name, item.Value);
                return new DynamicEntity(propertyValues);
            }

            return new DynamicEntity(null);
        }

        /// <summary>
        /// 构建动态实体
        /// </summary>
        /// <param name="propertyValues">"属性名-属性值"键值队列(仅提交第一个属性映射的表)</param>
        /// <param name="selfSheet">操作单子</param>
        public static DynamicEntity Fetch(IDictionary<string, object> propertyValues, Sheet selfSheet = null)
        {
            return new DynamicEntity(propertyValues != null ? new Dictionary<string, object>(propertyValues) : null, selfSheet);
        }

        /// <summary>
        /// 构建动态实体集合
        /// </summary>
        /// <param name="propertyValues">"属性名-属性值"键值队列(仅提交第一个属性映射的表)</param>
        /// <param name="selfSheet">操作单子</param>
        public static IList<DynamicEntity> FetchList(IList<IDictionary<string, object>> propertyValues, Sheet selfSheet = null)
        {
            IList<DynamicEntity> result = new List<DynamicEntity>();
            if (propertyValues != null)
                foreach (IDictionary<string, object> item in propertyValues)
                    result.Add(Fetch(item, selfSheet));
            return result;
        }

        #endregion

        #region 属性

        private readonly IDictionary<string, object> _propertyValues;

        /// <summary>
        /// "属性名-属性值"键值队列
        /// </summary>
        public IDictionary<string, object> PropertyValues
        {
            get { return _propertyValues; }
        }

        [NonSerialized]
        private Sheet _selfSheet;

        /// <summary>
        /// 操作单子
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Sheet SelfSheet
        {
            get { return _selfSheet; }
            set { _selfSheet = value; }
        }

        #endregion

        #region 方法

        private Sheet GetSelfSheet()
        {
            if (SelfSheet == null)
                throw new InvalidOperationException("属性 SelfSheet 不允许为空");
            return SelfSheet;
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="binder">动态设置属性操作</param>
        /// <param name="value">值</param>
        /// <returns>是否成功</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_propertyValues.ContainsKey(binder.Name))
            {
                _propertyValues[binder.Name] = value;
                return true;
            }

            if (SelfSheet != null)
            {
                Column tableColumn = SelfSheet.FindTableColumn(binder.Name);
                if (tableColumn != null)
                {
                    _propertyValues[binder.Name] = value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="binder">动态获取属性操作</param>
        /// <param name="result">值</param>
        /// <returns>是否成功</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _propertyValues.TryGetValue(binder.Name, out result);
        }

        /// <summary>
        /// 返回循环访问的枚举数
        /// </summary>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _propertyValues.GetEnumerator();
        }

        #region InsertRecord

        /// <summary>
        /// 新增记录(仅提交第一个属性映射的表)
        /// </summary>
        /// <returns>更新记录数</returns>
        public int InsertRecord()
        {
            return GetSelfSheet().InsertRecord(_propertyValues);
        }

        /// <summary>
        /// 新增记录(仅提交第一个属性映射的表)
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public int InsertRecord(DbConnection connection)
        {
            return GetSelfSheet().InsertRecord(connection, _propertyValues);
        }

        /// <summary>
        /// 新增记录(仅提交第一个属性映射的表)
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public int InsertRecord(DbTransaction transaction)
        {
            return GetSelfSheet().InsertRecord(transaction, _propertyValues);
        }

        #endregion

        #region UpdateRecord

        /// <summary>
        /// 更新记录(仅提交第一个属性映射的表)
        /// </summary>
        /// <returns>更新记录数</returns>
        public int UpdateRecord()
        {
            return GetSelfSheet().UpdateRecord(_propertyValues);
        }

        /// <summary>
        /// 更新记录(仅提交第一个属性映射的表)
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public int UpdateRecord(DbConnection connection)
        {
            return GetSelfSheet().UpdateRecord(connection, _propertyValues);
        }

        /// <summary>
        /// 更新记录(仅提交第一个属性映射的表)
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <returns>更新记录数</returns>
        public int UpdateRecord(DbTransaction transaction)
        {
            return GetSelfSheet().UpdateRecord(transaction, _propertyValues);
        }

        #endregion

        #region DeleteRecord

        /// <summary>
        /// 删除记录(仅提交第一个属性映射的表)
        /// </summary>
        /// <returns>删除记录数</returns>
        public int DeleteRecord()
        {
            return GetSelfSheet().DeleteRecord(_propertyValues);
        }

        /// <summary>
        /// 删除记录(仅提交第一个属性映射的表)
        /// </summary>
        /// <param name="connection">DbConnection(注意跨库风险未作校验)</param>
        /// <returns>删除记录数</returns>
        public int DeleteRecord(DbConnection connection)
        {
            return GetSelfSheet().DeleteRecord(connection, _propertyValues);
        }

        /// <summary>
        /// 删除记录(仅提交第一个属性映射的表)
        /// </summary>
        /// <param name="transaction">DbTransaction(注意跨库风险未作校验)</param>
        /// <returns>删除记录数</returns>
        public int DeleteRecord(DbTransaction transaction)
        {
            return GetSelfSheet().DeleteRecord(transaction, _propertyValues);
        }

        #endregion

        #endregion
    }
}
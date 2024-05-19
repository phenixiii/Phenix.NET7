using System;
using System.Reflection;
using Phenix.Core.SyncCollections;

namespace Phenix.Core.Reflection
{
    /// <summary>
    /// 实体信息
    /// </summary>
    public sealed class InstanceInfo
    {
        private InstanceInfo(Type objectType)
        {
            _objectType = objectType;
        }

        #region 工厂

        private static readonly SynchronizedDictionary<string, InstanceInfo> _cache = new SynchronizedDictionary<string, InstanceInfo>(StringComparer.Ordinal);

        /// <summary>
        /// 获取
        /// </summary>
        /// <returns>TypeInfo</returns>
        public static InstanceInfo Fetch<T>()
            where T : class
        {
            return Fetch(typeof(T));
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="objectType">类</param>
        /// <returns>TypeInfo</returns>
        public static InstanceInfo Fetch(Type objectType)
        {
            if (objectType == null)
                throw new ArgumentNullException(nameof(objectType));

            return _cache.GetValue(objectType.FullName, () => new InstanceInfo(objectType));
        }

        #endregion

        #region 属性

        private readonly Type _objectType;

        /// <summary>
        /// 类
        /// </summary>
        public Type ObjectType
        {
            get { return _objectType; }
        }

        private DynamicCtorDelegate _create;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DynamicCtorDelegate Create
        {
            get
            {
                if (_create == null)
                    _create = DynamicInstanceFactory.GetConstructor(_objectType);
                return _create;
            }
        }

        private readonly SynchronizedDictionary<string, DynamicMethodDelegate> _methodCache = new SynchronizedDictionary<string, DynamicMethodDelegate>(StringComparer.Ordinal);
        private readonly SynchronizedDictionary<string, DynamicMemberGetDelegate> _getCache = new SynchronizedDictionary<string, DynamicMemberGetDelegate>(StringComparer.Ordinal);
        private readonly SynchronizedDictionary<string, DynamicMemberSetDelegate> _setCache = new SynchronizedDictionary<string, DynamicMemberSetDelegate>(StringComparer.Ordinal);

        #endregion

        #region 方法

        /// <summary>
        /// 动态执行函数
        /// </summary>
        public DynamicMethodDelegate CreateMethod(MethodInfo method)
        {
            return _methodCache.GetValue(method.Name, () => DynamicInstanceFactory.CreateMethod(method));
        }

        /// <summary>
        /// 动态执行属性的get函数
        /// </summary>
        public DynamicMemberGetDelegate CreatePropertyGetter(PropertyInfo property)
        {
            return _getCache.GetValue(property.Name, () => DynamicInstanceFactory.CreatePropertyGetter(property));
        }

        /// <summary>
        /// 动态执行属性的set函数
        /// </summary>
        public DynamicMemberSetDelegate CreatePropertySetter(PropertyInfo property)
        {
            return _setCache.GetValue(property.Name, () => DynamicInstanceFactory.CreatePropertySetter(property));
        }

        /// <summary>
        /// 动态执行字段的get函数
        /// </summary>
        public DynamicMemberGetDelegate CreateFieldGetter(FieldInfo field)
        {
            return _getCache.GetValue(field.Name, () => DynamicInstanceFactory.CreateFieldGetter(field));
        }

        /// <summary>
        /// 动态执行字段的set函数
        /// </summary>
        public DynamicMemberSetDelegate CreateFieldSetter(FieldInfo field)
        {
            return _setCache.GetValue(field.Name, () => DynamicInstanceFactory.CreateFieldSetter(field));
        }

        #endregion
    }
}

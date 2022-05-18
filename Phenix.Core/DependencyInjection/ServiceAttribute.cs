using System;
using Microsoft.Extensions.DependencyInjection;

namespace Phenix.Core.DependencyInjection
{
    /// <summary>
    /// 服务标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ServiceAttribute : Attribute
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="interfaceType">服务接口</param>
        /// <param name="lifetime">生命周期</param>
        public ServiceAttribute(Type interfaceType, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            _interfaceType = interfaceType;
            _lifetime = lifetime;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="lifetime">ServiceLifetime</param>
        public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            : this(null, lifetime)
        {
        }

        #region 属性

        private readonly Type _interfaceType;

        /// <summary>
        /// 服务接口
        /// </summary>
        public Type InterfaceType
        {
            get { return _interfaceType; }
        }

        private readonly ServiceLifetime _lifetime;

        /// <summary>
        /// 生命周期
        /// </summary>
        public ServiceLifetime Lifetime
        {
            get { return _lifetime; }
        }

        #endregion
    }
}
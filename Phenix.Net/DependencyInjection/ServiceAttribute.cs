using System;
using Microsoft.Extensions.DependencyInjection;

namespace Phenix.Net.DependencyInjection
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
        /// <param name="lifetime">ServiceLifetime</param>
        public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            _lifetime = lifetime;
        }

        #region 属性

        private readonly ServiceLifetime _lifetime;

        /// <summary>
        /// ServiceLifetime
        /// </summary>
        public ServiceLifetime Lifetime
        {
            get { return _lifetime; }
        }

        #endregion
    }
}
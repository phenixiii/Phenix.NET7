using System;
using System.Reflection;
using Phenix.Core.Log;
using Phenix.Core.Reflection;
using Phenix.Core.SyncCollections;

namespace Phenix.Core.Plugin
{
    /// <summary>
    /// 插件容器
    /// </summary>
    public sealed class PluginHost
    {
        #region 单例

        private static readonly object _defaultLock = new object();
        private static PluginHost _default;

        /// <summary>
        /// 单例
        /// </summary>
        public static PluginHost Default
        {
            get
            {
                if (_default == null)
                    lock (_defaultLock)
                        if (_default == null)
                        {
                            _default = new PluginHost();
                        }

                return _default;
            }
        }

        #endregion

        #region 属性

        private readonly SynchronizedDictionary<string, IPlugin> _cache = new SynchronizedDictionary<string, IPlugin>(StringComparer.Ordinal);

        #endregion

        #region 方法

        /// <summary>
        /// 获取插件
        /// </summary>
        /// <param name="assemblyName">程序集名</param>
        /// <param name="onMessage">插件发送过来的消息</param>
        public IPlugin GetPlugin(string assemblyName, Func<IPlugin, object, object> onMessage = null)
        {
            return GetPlugin(Utilities.LoadAssembly(assemblyName, true), onMessage);
        }

        /// <summary>
        /// 获取插件
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="onMessage">插件发送过来的消息</param>
        public IPlugin GetPlugin(Assembly assembly, Func<IPlugin, object, object> onMessage = null)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            return _cache.GetValue(assembly.GetName().Name, () =>
            {
                try
                {
                    return PluginBase.New(assembly, this, onMessage);
                }
                catch (Exception ex)
                {
                    LogHelper.Warning(ex.Message);
                    throw;
                }
            });
        }

        #endregion
    }
}
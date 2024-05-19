using System;
using System.Reflection;
using Phenix.Core.Reflection;
using Phenix.Core.SyncCollections;

namespace Phenix.Core.Plugin
{
    /// <summary>
    /// 插件抽象类
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        #region 工厂

        internal static PluginBase New(Assembly assembly, PluginHost owner, Func<IPlugin, object, object> onMessage)
        {
            PluginBase result = (PluginBase)InstanceInfo.Fetch(FindPluginType(assembly, true)).Create();
            result._owner = owner;
            result._onMessage = onMessage;
            return result;
        }

        #endregion

        #region 属性

        private PluginHost _owner;

        /// <summary>
        /// 插件容器
        /// </summary>
        protected PluginHost Owner
        {
            get { return _owner; }
        }

        private PluginState _state;

        /// <summary>
        /// 插件状态
        /// </summary>
        public PluginState State
        {
            get { return _state; }
            private set { _state = value; }
        }

        private Func<IPlugin, object, object> _onMessage;

        private static readonly SynchronizedDictionary<string, Type> _typeCache = new SynchronizedDictionary<string, Type>(StringComparer.Ordinal);

        #endregion

        #region 方法

        /// <summary>
        /// 检索插件类
        /// </summary>
        /// <param name="assemblyName">程序集名称</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 ArgumentException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <returns>插件类</returns>
        public static Type FindPluginType(string assemblyName, bool throwIfNotFound = false)
        {
            return FindPluginType(Utilities.LoadAssembly(assemblyName, throwIfNotFound), throwIfNotFound);
        }

        /// <summary>
        /// 检索插件类
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 ArgumentException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <returns>插件类</returns>
        public static Type FindPluginType(Assembly assembly, bool throwIfNotFound = false)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            return _typeCache.GetValue(assembly.GetName().Name, () =>
            {
                foreach (Type item in assembly.GetExportedTypes())
                {
                    if (!item.IsClass || item.IsAbstract || item.IsGenericType || item.IsCOMObject)
                        continue;
                    if (typeof(IPlugin).IsAssignableFrom(item))
                        return item;
                }

                if (throwIfNotFound)
                    throw new InvalidOperationException(String.Format("程序集 {0} 无插件类", assembly.FullName));
                return null;
            });
        }

        #region IPlugin 成员

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns>确定启动</returns>
        protected virtual bool Start()
        {
            return true;
        }

        bool IPlugin.Start()
        {
            bool result = Start();
            if (result)
                State = PluginState.Started;
            return result;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns>确定停止</returns>
        protected virtual bool Suspend()
        {
            return true;
        }

        bool IPlugin.Suspend()
        {
            bool result = Suspend();
            if (result)
                State = PluginState.Suspended;
            return result;
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>按需返回</returns>
        public virtual object ReceiveMessage(object message)
        {
            return this;
        }

        #endregion

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>按需返回</returns>
        public object SendMessage(object message)
        {
            return _onMessage != null ? _onMessage(this, message) : null;
        }

        #endregion
    }
}
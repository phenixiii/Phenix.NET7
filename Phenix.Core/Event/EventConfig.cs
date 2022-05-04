namespace Phenix.Core.Event
{
    /// <summary>
    /// 事件配置信息
    /// </summary>
    public static class EventConfig
    {
        private static string _pubSubName;

        /// <summary>
        /// PubSub名称
        /// 默认：pubsub（对应 pubsub.yaml 中 name: pubsub）
        /// </summary>
        public static string PubSubName
        {
            get { return AppSettings.GetLocalProperty(ref _pubSubName, "pubsub"); }
            set { AppSettings.SetLocalProperty(ref _pubSubName, value); }
        }
    }
}
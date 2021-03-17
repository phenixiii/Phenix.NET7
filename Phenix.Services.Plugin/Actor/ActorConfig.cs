using System;

namespace Phenix.Services.Plugin.Actor
{
    /// <summary>
    ///  Actor配置信息
    /// </summary>
    public static class ActorConfig
    {
        private static readonly Guid _groupMessageStreamId = new Guid("54E67F1B-ABD7-4518-BA30-020A20AE3D49");

        /// <summary>
        /// 分组消息StreamId
        /// </summary>
        public static Guid GroupMessageStreamId
        {
            get { return _groupMessageStreamId; }
        }
    }
}
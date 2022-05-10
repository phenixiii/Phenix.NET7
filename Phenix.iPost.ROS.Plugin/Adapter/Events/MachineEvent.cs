using System;
using Phenix.Core.Event;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events
{
    /// <summary>
    /// 机械事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    [Serializable]
    public record MachineEvent(
            string MachineId)
        : IntegrationEvent;
}

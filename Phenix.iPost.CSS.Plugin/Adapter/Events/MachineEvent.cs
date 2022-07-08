using System;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events
{
    /// <summary>
    /// 机械事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    [Serializable]
    public record MachineEvent(
            string MachineId,
            MachineType MachineType)
        : IntegrationEvent;
}

using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 机械任务反馈事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    /// <param name="TaskId">任务ID</param>
    /// <param name="TaskStatus">任务状态</param>
    [Serializable]
    public record MachineTaskAckEvent(string MachineId, MachineType MachineType, string TaskId, TaskStatus TaskStatus)
        : MachineTaskEvent(MachineId, MachineType, TaskId, TaskStatus);
}
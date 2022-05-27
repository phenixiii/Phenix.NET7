﻿using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events
{
    /// <summary>
    /// 机械任务事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="TaskId">任务ID</param>
    /// <param name="TaskStatus">任务状态</param>
    [Serializable]
    public record MachineTaskEvent(string MachineId,
            string TaskId,
            TaskStatus TaskStatus)
        : MachineEvent(MachineId);
}
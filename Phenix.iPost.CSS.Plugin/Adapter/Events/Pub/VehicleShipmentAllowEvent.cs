﻿using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Pub
{
    /// <summary>
    /// 拖车装船允许事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    /// <param name="TaskId">任务ID</param>
    /// <param name="TaskStatus">任务状态</param>
    /// <param name="Loadable">允许上档</param>
    /// <param name="Order">上档次序</param>
    [Serializable]
    public record VehicleShipmentAllowEvent(string MachineId, MachineType MachineType, string TaskId, TaskStatus TaskStatus,
            bool Loadable,
            int Order)
        : MachineTaskEvent(MachineId, MachineType, TaskId, TaskStatus);
}
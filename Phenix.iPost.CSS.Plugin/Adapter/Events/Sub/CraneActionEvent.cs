using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 吊车动作事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    /// <param name="CraneType">吊车类型</param>
    /// <param name="CraneAction">吊车动作</param>
    [Serializable]
    public record CraneActionEvent(string MachineId, MachineType MachineType,
            CraneType CraneType,
            CraneAction CraneAction)
        : MachineEvent(MachineId, MachineType);
}
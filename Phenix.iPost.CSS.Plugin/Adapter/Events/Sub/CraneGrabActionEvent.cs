using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 吊车抓具动作事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    /// <param name="CraneType">吊车类型</param>
    /// <param name="GrabAction">抓具动作</param>
    /// <param name="HoistHeight">起升高度cm</param>
    [Serializable]
    public record CraneGrabActionEvent(string MachineId, MachineType MachineType,
            CraneType CraneType,
            CraneGrabAction GrabAction,
            int HoistHeight)
        : MachineEvent(MachineId, MachineType);
}
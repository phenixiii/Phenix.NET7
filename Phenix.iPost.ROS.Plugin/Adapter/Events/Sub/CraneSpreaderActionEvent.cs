using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 吊车吊具动作事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="SpreaderAction">吊具动作</param>
    /// <param name="SpreaderSize">吊具尺寸</param>
    /// <param name="HoistHeight">起升高度cm</param>
    [Serializable]
    public record CraneSpreaderActionEvent(string MachineId,
            CraneSpreaderAction SpreaderAction,
            CraneSpreaderSize SpreaderSize,
            int HoistHeight)
        : MachineEvent(MachineId);
}
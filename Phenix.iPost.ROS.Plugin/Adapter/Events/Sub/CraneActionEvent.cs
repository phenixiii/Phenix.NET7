using System;
using Phenix.iPost.ROS.Plugin.Adapter.Norms;

namespace Phenix.iPost.ROS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 吊车动作事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许ID重复）</param>
    /// <param name="CraneAction">吊车动作</param>
    /// <param name="Location">Action=ArriveBay时有意义为所在贝位</param>
    [Serializable]
    public record CraneActionEvent(string MachineId,
            CraneAction CraneAction,
            string Location)
        : MachineEvent(MachineId);
}
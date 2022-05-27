using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 动力属性
    /// </summary>
    /// <param name="PowerType">动力类型</param>
    /// <param name="PowerStatus">动力状态</param>
    /// <param name="SurplusCapacityPercent">剩余容量百分比</param>
    [Serializable]
    public readonly record struct PowerProperty(
        PowerType PowerType,
        PowerStatus PowerStatus,
        int? SurplusCapacityPercent);
}
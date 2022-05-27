using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 靠泊属性
    /// </summary>
    /// <param name="AlongSide">靠泊方向</param>
    /// <param name="BowBollardId">船头缆桩号</param>
    /// <param name="BowBollardOffset">船头缆桩偏差值cm</param>
    /// <param name="SternBollardId">船尾缆桩号</param>
    /// <param name="SternBollardOffset">船尾缆桩偏差值cm</param>
    [Serializable]
    public readonly record struct VesselAlongSideProperty(
        VesselAlongSide AlongSide,
        string BowBollardId,
        string BowBollardOffset,
        string SternBollardId,
        string SternBollardOffset
    );
}
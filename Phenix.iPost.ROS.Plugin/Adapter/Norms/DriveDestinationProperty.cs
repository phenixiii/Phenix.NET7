using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 驱车目的地属性
    /// </summary>
    /// <param name="DestinationId">目的地ID（桥吊号/箱区名/锁站/电桩/停车位，全域唯一，即可控生产区域内各类目的地之间都不允许ID重复）</param>
    /// <param name="Bay">贝（目的地是桥吊、箱区才有意义）</param>
    /// <param name="Lane">车道（目的地是桥吊、箱区、锁站才有意义）</param>
    [Serializable]
    public record DriveDestinationProperty(
        string DestinationId,
        string Bay,
        string Lane);
}

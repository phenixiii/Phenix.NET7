using System;

namespace Phenix.iPost.CSS.Plugin.Adapter.Norms
{
    /// <summary>
    /// 机械类型
    /// </summary>
    [Serializable]
    public enum MachineType
    {
        /// <summary>
        /// 岸桥
        /// </summary>
        QuayCrane,

        /// <summary>
        /// 场桥
        /// </summary>
        YardCrane,

        /// <summary>
        /// 拖车
        /// </summary>
        Vehicle,
    }
}

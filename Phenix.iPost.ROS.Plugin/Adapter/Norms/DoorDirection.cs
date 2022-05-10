using System;

namespace Phenix.iPost.ROS.Plugin.Message.Norm
{
    /// <summary>
    /// 箱门方向
    /// </summary>
    [Serializable]
    public enum DoorDirection
    {
        LowBollard,

        HighBollard,

        LowRow,
        
        HighRow
    }
}

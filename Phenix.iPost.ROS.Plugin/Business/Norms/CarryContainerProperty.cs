using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 载箱属性
    /// </summary>
    /// <param name="Position">载箱位置</param>
    /// <param name="ContainerId">箱号</param>
    /// <param name="IsoCode">箱型</param>
    [Serializable]
    public readonly record struct CarryContainerProperty(
        CarryContainerPosition Position,
        string ContainerId,
        string IsoCode);
}

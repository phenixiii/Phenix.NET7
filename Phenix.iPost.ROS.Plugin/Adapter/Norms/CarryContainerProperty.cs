using System;

namespace Phenix.iPost.ROS.Plugin.Adapter.Norms
{
    /// <summary>
    /// （计划/实装）载箱属性
    /// </summary>
    /// <param name="Position">载箱位置</param>
    /// <param name="ContainerId">箱号</param>
    /// <param name="IsoCode">箱型</param>
    [Serializable]
    public record CarryContainerProperty(
        CarryContainerPosition Position,
        string ContainerId,
        string IsoCode);
}

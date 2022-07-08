using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business.Property
{
    /// <summary>
    /// 载货属性
    /// </summary>
    /// <param name="ContainerNumber">箱号</param>
    /// <param name="ContainerSize">箱尺寸</param>
    /// <param name="CarryPosition">载运位置</param>
    [Serializable]
    public readonly record struct CarryCargoProperty(
        string ContainerNumber,
        string ContainerSize,
        CarryPosition CarryPosition);
}

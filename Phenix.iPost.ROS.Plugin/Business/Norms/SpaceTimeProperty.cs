using System;

namespace Phenix.iPost.ROS.Plugin.Business.Norms
{
    /// <summary>
    /// 时空属性
    /// </summary>
    /// <param name="X">位置X坐标</param>
    /// <param name="Y">位置Y坐标</param>
    /// <param name="Speed">速度m/s</param>
    /// <param name="Longitude">经度E</param>
    /// <param name="Latitude">纬度S</param>
    /// <param name="Heading">航向角</param>
    [Serializable]
    public readonly record struct SpaceTimeProperty(
        float X,
        float Y,
        float? Speed = null,
        string Longitude = null,
        string Latitude = null,
        float? Heading = null);
}
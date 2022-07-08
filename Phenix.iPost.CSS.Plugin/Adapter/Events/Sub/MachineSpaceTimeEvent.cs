using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 机械时空事件
    /// </summary>
    /// <param name="MachineId">机械ID（全域唯一，即可控生产区域内各类机械之间都不允许重复）</param>
    /// <param name="MachineType">机械类型</param>
    /// <param name="X">位置X坐标</param>
    /// <param name="Y">位置Y坐标</param>
    /// <param name="Speed">速度m/s</param>
    /// <param name="Longitude">经度E</param>
    /// <param name="Latitude">纬度S</param>
    /// <param name="Heading">航向角</param>
    [Serializable]
    public record MachineSpaceTimeEvent(string MachineId, MachineType MachineType,
            float X,
            float Y,
            float? Speed,
            string Longitude,
            string Latitude,
            float? Heading)
        : MachineEvent(MachineId, MachineType);
}
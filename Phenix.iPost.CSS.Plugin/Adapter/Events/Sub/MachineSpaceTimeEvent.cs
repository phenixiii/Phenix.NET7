﻿using System;
using Phenix.iPost.CSS.Plugin.Adapter.Norms;

namespace Phenix.iPost.CSS.Plugin.Adapter.Events.Sub
{
    /// <summary>
    /// 机械时空事件
    /// </summary>
    [Serializable]
    public record MachineSpaceTimeEvent : MachineEvent
    {
        /// <summary>
        /// 机械时空事件
        /// </summary>
        /// <param name="machineId">机械ID</param>
        /// <param name="machineType">机械类型</param>
        /// <param name="x">位置X坐标</param>
        /// <param name="y">位置Y坐标</param>
        /// <param name="location">所在位置</param>
        /// <param name="speed">速度m/s</param>
        /// <param name="longitude">经度E</param>
        /// <param name="latitude">纬度S</param>
        /// <param name="heading">航向角</param>
        [Newtonsoft.Json.JsonConstructor]
        public MachineSpaceTimeEvent(string machineId, MachineType machineType,
            float x, float y, string location,
            float? speed, string longitude, string latitude, float? heading)
            : base(machineId, machineType)
        {
            this.X = x;
            this.Y = y;
            this.Location = location;
            this.Speed = speed;
            this.Longitude = longitude;
            this.Latitude = latitude;
            this.Heading = heading;
        }

        #region 属性

        /// <summary>
        /// 位置X坐标
        /// </summary>
        public float X { get; }

        /// <summary>
        /// 位置Y坐标
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// 所在位置
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// 速度m/s
        /// </summary>
        public float? Speed { get; }

        /// <summary>
        /// 经度E
        /// </summary>
        public string Longitude { get; }

        /// <summary>
        /// 纬度S
        /// </summary>
        public string Latitude { get; }

        /// <summary>
        /// 航向角
        /// </summary>
        public float? Heading { get; }

        #endregion
    }
}
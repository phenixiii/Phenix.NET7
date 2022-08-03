using System;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 时空属性
    /// </summary>
    [Serializable]
    public readonly record struct SpaceTimeInfo
    {
        /// <summary>
        /// 时空属性
        /// </summary>
        /// <param name="x">位置X坐标</param>
        /// <param name="y">位置Y坐标</param>
        /// <param name="location"></param>
        /// <param name="speed">速度m/s</param>
        /// <param name="longitude">经度E</param>
        /// <param name="latitude">纬度S</param>
        /// <param name="heading">航向角</param>
        [Newtonsoft.Json.JsonConstructor]
        public SpaceTimeInfo(float x, float y, string location,
            float? speed = null, string longitude = null, string latitude = null, float? heading = null)
        {
            X = x;
            Y = y;
            Location = location;
            Speed = speed;
            Longitude = longitude;
            Latitude = latitude;
            Heading = heading;
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
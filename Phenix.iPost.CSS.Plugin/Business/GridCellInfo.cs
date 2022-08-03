using System;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 栅格单元
    /// </summary>
    [Serializable]
    public readonly record struct GridCellInfo
    {
        internal GridCellInfo(float x, float y, string location = null)
            : this((int)Math.Round(x), (int)Math.Round(y), location)
        {
        }

        /// <summary>
        /// 栅格单元
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="location">所在位置</param>
        [Newtonsoft.Json.JsonConstructor]
        public GridCellInfo(int x, int y, string location = null)
        {
            this.X = x;
            this.Y = y;
            this.Location = location;
        }

        #region 属性

        /// <summary>
        /// 唯一键
        /// X+Y
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string Key => $"{X}+{Y}";

        /// <summary>
        /// X坐标
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y坐标
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// 所在位置
        /// </summary>
        public string Location { get; }

        #endregion
    }
}
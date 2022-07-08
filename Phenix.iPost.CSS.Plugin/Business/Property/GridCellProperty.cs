using System;

namespace Phenix.iPost.CSS.Plugin.Business.Property
{
    /// <summary>
    /// 栅格单元属性
    /// </summary>
    /// <param name="X">X坐标</param>
    /// <param name="Y">Y坐标</param>
    /// <param name="Location">所在位置</param>
    [Serializable]
    public readonly record struct GridCellProperty(int X, int Y, string Location = null)
    {
        /// <summary>
        /// 栅格单元
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="location">所在位置</param>
        public GridCellProperty(float x, float y, string location = null)
            : this((int)Math.Round(x), (int)Math.Round(y), location)
        {
        }

        #region 属性

        /// <summary>
        /// 唯一键
        /// X+Y
        /// </summary>
        public string Key
        {
            get { return $"{X}+{Y}"; }
        }

        #endregion
    }
}
using System;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin.Business
{
    /// <summary>
    /// 载货
    /// </summary>
    [Serializable]
    public readonly record struct CarryCargoInfo
    {
        /// <summary>
        /// 载货属性
        /// </summary>
        /// <param name="containerNumber">箱号</param>
        /// <param name="containerSize">箱尺寸</param>
        /// <param name="carryPosition">载运位置</param>
        [Newtonsoft.Json.JsonConstructor]
        public CarryCargoInfo(string containerNumber, string containerSize, CarryPosition carryPosition)
        {
            this.ContainerNumber = containerNumber;
            this.ContainerSize = containerSize;
            this.CarryPosition = carryPosition;
        }

        #region 属性

        /// <summary>
        /// 箱号
        /// </summary>
        public string ContainerNumber { get;}

        /// <summary>
        /// 箱尺寸
        /// </summary>
        public string ContainerSize { get;  }

        /// <summary>
        /// 载运位置
        /// </summary>
        public CarryPosition CarryPosition { get;  }

        #endregion
    }
}

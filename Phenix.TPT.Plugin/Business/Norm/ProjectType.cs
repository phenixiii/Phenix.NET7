using System;
using Phenix.Core.Data;

namespace Phenix.TPT.Plugin.Business.Norm
{
    /// <summary>
    /// 项目类型
    /// </summary>
    [Serializable]
    public enum ProjectType
    {
        /// <summary>
        /// 技术服务
        /// </summary>
        [EnumCaption("技术服务")]
        技术服务 = 0,

        /// <summary>
        /// 集成服务
        /// </summary>
        [EnumCaption("集成服务")]
        集成服务 = 1,

        /// <summary>
        /// 运维服务
        /// </summary>
        [EnumCaption("运维服务")]
        运维服务 = 2,

        /// <summary>
        /// 产品研发
        /// </summary>
        [EnumCaption("产品研发")]
        产品研发 = 3,
    }
}
using System;
using Phenix.Core.Data;

namespace Phenix.TPT.Business.Norm
{
    /// <summary>
    /// 项目类型
    /// </summary>
    [Serializable]
    public enum ProjectType
    {
        [EnumCaption("技术服务")]
        技术服务 = 0,

        [EnumCaption("集成服务")]
        集成服务 = 1,

        [EnumCaption("运维服务")]
        运维服务 = 2,

        [EnumCaption("产品研发")]
        产品研发 = 3,
    }
}
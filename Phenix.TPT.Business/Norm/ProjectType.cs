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
        [EnumCaption("软件技术服务")]
        软件技术服务 = 0,

        [EnumCaption("集成技术服务")]
        集成技术服务 = 1,

        [EnumCaption("运维服务")]
        运维服务 = 2,

        [EnumCaption("产品研发")]
        产品研发 = 3,
    }
}
using System;

namespace Phenix.TPT.Business.Norm
{
    /// <summary>
    /// 项目类型
    /// </summary>
    [Serializable]
    public enum ProjectType
    {
        /// <summary>
        /// 软件项目
        /// </summary>
        [Phenix.Core.Data.EnumCaption("软件项目")]
        SoftwareProject = 0,

        /// <summary>
        /// 集成项目
        /// </summary>
        [Phenix.Core.Data.EnumCaption("集成项目")]
        IntegrationProject = 1,

        /// <summary>
        /// 研发任务
        /// </summary>
        [Phenix.Core.Data.EnumCaption("研发任务")]
        ResearchTask = 2,
    }
}
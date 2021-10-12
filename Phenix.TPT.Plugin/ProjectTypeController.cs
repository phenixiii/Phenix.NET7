using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Data;
using Phenix.TPT.Business.Norm;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 项目类型控制器
    /// </summary>
    [Route(WebApiConfig.ProjectTypePath)]
    [ApiController]
    public sealed class ProjectTypeController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        /// <summary>
        /// 获取项目类型
        /// </summary>
        [Authorize]
        [HttpGet("all")]
        public IList<EnumKeyValue> GetAll()
        {
            return EnumKeyValue.Fetch<ProjectType>();
        }

        #endregion
    }
}

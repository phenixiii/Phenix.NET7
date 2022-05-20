using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Data;
using Phenix.TPT.Plugin.Business.Norm;

namespace Phenix.TPT.Plugin.WebApi
{
    /// <summary>
    /// 项目类型控制器
    /// </summary>
    [Route(WebApiPaths.ProjectTypePath)]
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

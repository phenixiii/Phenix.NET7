using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Data;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 项目工作人员控制器
    /// </summary>
    [Route(WebApiConfig.ProjectWorkerPath)]
    [ApiController]
    public sealed class ProjectWorkerController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        /// <summary>
        /// 获取项目工作人员
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        [Authorize]
        [HttpGet("all")]
        public IList<ProjectWorkerV> GetAll(short year, short month)
        {
            return ProjectWorkerV.FetchList(Database.Default, p => p.Year == year && p.Month == month);
        }

        #endregion
    }
}

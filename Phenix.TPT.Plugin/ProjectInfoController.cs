using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 项目资料控制器
    /// </summary>
    [Route(WebApiConfig.ProjectInfoPath)]
    [ApiController]
    public sealed class ProjectInfoController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        [Authorize]
        [HttpGet]
        public IList<ProjectInfo> Get(int year, int month)
        {
            DateTime firstDay = new DateTime(year, month, 1);
            DateTime lastDay = month < 12
                ? new DateTime(year, month + 1, 1).AddMilliseconds(-1)
                : new DateTime(year + 1, 1, 1).AddMilliseconds(-1);
            return ProjectInfo.FetchList(Database.Default,
                p => p.OriginateTime <= lastDay && (p.ClosedDate == null || p.ClosedDate >= firstDay),
                OrderBy.Descending<ProjectInfo>(p => p.ContApproveDate));
        }

        #endregion
    }
}

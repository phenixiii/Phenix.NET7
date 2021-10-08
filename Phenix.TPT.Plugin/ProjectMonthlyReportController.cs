﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;
using Phenix.TPT.Plugin.Filters;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 项目月报控制器
    /// </summary>
    [Route(WebApiConfig.ProjectMonthlyReportPath)]
    [ApiController]
    public sealed class ProjectMonthlyReportController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        [Authorize]
        [HttpGet]
        public async Task<ProjectMonthlyReport> Get(long projectId, int year, int month)
        {
            return await ClusterClient.Default.GetGrain<IProjectGrain>(projectId).GetProjectMonthlyReport(year, month);
        }

        [ProjectControlFilter]
        [Authorize]
        [HttpPut]
        public async Task Put(long projectId)
        {
            await ClusterClient.Default.GetGrain<IProjectGrain>(projectId).PutProjectMonthlyReport(await Request.ReadBodyAsync<ProjectMonthlyReport>());
        }

        #endregion
    }
}

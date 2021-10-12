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

        /// <summary>
        /// 获取项目月报(如不存在则返回初始对象)
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>项目月报</returns>
        [Authorize]
        [HttpGet]
        public async Task<ProjectMonthlyReport> Get(long projectId, short year, short month)
        {
            return await ClusterClient.Default.GetGrain<IProjectGrain>(projectId).GetProjectMonthlyReport(year, month);
        }

        /// <summary>
        /// 更新项目月报(如不存在则新增)
        /// </summary>
        [ProjectControlFilter]
        [Authorize]
        [HttpPut]
        public async Task Put()
        {
            ProjectMonthlyReport projectMonthlyReport = await Request.ReadBodyAsync<ProjectMonthlyReport>();
            await ClusterClient.Default.GetGrain<IProjectGrain>(projectMonthlyReport.PiId).PutProjectMonthlyReport(projectMonthlyReport);
        }

        #endregion
    }
}

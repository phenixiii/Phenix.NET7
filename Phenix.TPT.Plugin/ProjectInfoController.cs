using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;
using Phenix.TPT.Plugin.Filters;

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
        [HttpGet("all")]
        public IList<ProjectInfo> GetAll(int year, int month)
        {
            DateTime firstDay = new DateTime(year, month, 1);
            DateTime lastDay = month < 12
                ? new DateTime(year, month + 1, 1).AddMilliseconds(-1)
                : new DateTime(year + 1, 1, 1).AddMilliseconds(-1);
            return ProjectInfo.FetchList(Database.Default,
                p => p.OriginateTime <= lastDay && (p.ClosedDate == null || p.ClosedDate >= firstDay),
                OrderBy.Descending<ProjectInfo>(p => p.ContApproveDate).
                    Descending(p => p.UpdateTime));
        }

        [Authorize]
        [HttpGet]
        public async Task<ProjectInfo> Get(long? id)
        {
            return await ClusterClient.Default.GetGrain<IProjectGrain>(id ?? Database.Default.Sequence.Value).FetchKernel(true);
        }

        [ProjectControlFilter]
        [HttpPut]
        public async Task Put()
        {
            ProjectInfo projectInfo = await Request.ReadBodyAsync<ProjectInfo>();
            await ClusterClient.Default.GetGrain<IProjectGrain>(projectInfo.Id).PutKernel(projectInfo);
        }

        [ProjectControlFilter]
        [HttpDelete]
        public async Task Close(long id, DateTime closedDate)
        {
            await ClusterClient.Default.GetGrain<IProjectGrain>(id).Close(closedDate);
        }

        #endregion
    }
}

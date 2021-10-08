using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;
using Phenix.TPT.Plugin.Filters;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 项目年度计划控制器
    /// </summary>
    [Route(WebApiConfig.ProjectAnnualPlanPath)]
    [ApiController]
    public sealed class ProjectAnnualPlanController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        [Authorize]
        [HttpGet]
        public async Task<ProjectAnnualPlan> Get(long projectId, int year)
        {
            return await ClusterClient.Default.GetGrain<IProjectGrain>(projectId).GetProjectAnnualPlan(year);
        }

        [ProjectControlFilter]
        [Authorize]
        [HttpPut]
        public async Task Put(long projectId)
        {
            await ClusterClient.Default.GetGrain<IProjectGrain>(projectId).PutProjectAnnualPlan(await Request.ReadBodyAsync<ProjectAnnualPlan>());
        }

        #endregion
    }
}

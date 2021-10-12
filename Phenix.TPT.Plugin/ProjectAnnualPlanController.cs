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

        /// <summary>
        /// 获取项目年度计划(如不存在则返回初始对象)
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="year">年</param>
        [Authorize]
        [HttpGet]
        public async Task<ProjectAnnualPlan> Get(long projectId, short year)
        {
            return await ClusterClient.Default.GetGrain<IProjectGrain>(projectId).GetProjectAnnualPlan(year);
        }

        /// <summary>
        /// 更新项目年度计划(如不存在则新增)
        /// </summary>
        [ProjectControlFilter]
        [Authorize]
        [HttpPut]
        public async Task Put()
        {
            ProjectAnnualPlan projectAnnualPlan = await Request.ReadBodyAsync<ProjectAnnualPlan>();
            await ClusterClient.Default.GetGrain<IProjectGrain>(projectAnnualPlan.PiId).PutProjectAnnualPlan(projectAnnualPlan);
        }

        #endregion
    }
}

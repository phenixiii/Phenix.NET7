using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 项目工作量控制器
    /// </summary>
    [Route(WebApiConfig.ProjectWorkloadPath)]
    [ApiController]
    public sealed class ProjectWorkloadController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        /// <summary>
        /// 获取项目工作量(如不存在则返回初始对象)
        /// </summary>
        /// <param name="worker">打工人</param>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>工作档期</returns>
        [Authorize]
        [HttpGet]
        public async Task<IList<ProjectWorkload>> Get(long worker, short year, short month)
        {
            return await ClusterClient.Default.GetGrain<IWorkerGrain>(worker).GetProjectWorkloads(year, month);
        }

        #endregion
    }
}

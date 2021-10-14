using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
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
        [Authorize]
        [HttpGet("all")]
        public async Task<IList<ProjectWorkload>> GetAll(long worker, short year, short month)
        {
            return await ClusterClient.Default.GetGrain<IProjectWorkloadGrain>(worker, Standards.FormatYearMonth(year, month).ToString(CultureInfo.InvariantCulture)).GetProjectWorkloads();
        }

        /// <summary>
        /// 更新项目工作量(如不存在则新增)
        /// </summary>
        [Authorize]
        [HttpPut]
        public async Task Put()
        {
            ProjectWorkload projectWorkload = await Request.ReadBodyAsync<ProjectWorkload>();
            await ClusterClient.Default.GetGrain<IProjectWorkloadGrain>(projectWorkload.Worker, Standards.FormatYearMonth(projectWorkload.Year, projectWorkload.Month).ToString(CultureInfo.InvariantCulture)).PutProjectWorkload(projectWorkload);
        }
        
        #endregion
    }
}

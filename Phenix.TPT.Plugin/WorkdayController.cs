using System;
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
    /// 工作日控制器
    /// </summary>
    [Route(WebApiConfig.WorkdayPath)]
    [ApiController]
    public sealed class WorkdayController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        /// <summary>
        /// 获取工作日
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>工作日</returns>
        [Authorize]
        [HttpGet]
        public async Task<Workday> Get(short year, short month)
        {
            return await ClusterClient.Default.GetGrain<IWorkdayGrain>(year).GetCurrentYearWorkday(month);
        }

        /// <summary>
        /// 更新工作日(如不存在则新增)
        /// </summary>
        [ProjectControlFilter]
        [Authorize]
        [HttpPut]
        public async Task Put()
        {
            Workday workday = await Request.ReadBodyAsync<Workday>();
            await ClusterClient.Default.GetGrain<IWorkdayGrain>(DateTime.Today.Year).PutWorkday(workday);
        }

        #endregion
    }
}

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
    /// 工作档期控制器
    /// </summary>
    [Route(WebApiConfig.WorkSchedulePath)]
    [ApiController]
    public sealed class WorkScheduleController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        /// <summary>
        /// 获取工作档期(如不存在则返回初始对象)
        /// </summary>
        /// <param name="manager">管理人员</param>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        [Authorize]
        [HttpGet]
        public async Task<WorkSchedule> Get(long manager, short year, short month)
        {
            return await ClusterClient.Default.GetGrain<IWorkScheduleGrain>(manager, Standards.FormatYearMonth(year, month).ToString(CultureInfo.InvariantCulture)).FetchKernel(true);
        }

        /// <summary>
        /// 更新工作档期(如不存在则新增)
        /// </summary>
        [Authorize]
        [HttpPut]
        public async Task Put()
        {
            WorkSchedule workSchedule = await Request.ReadBodyAsync<WorkSchedule>();
            await ClusterClient.Default.GetGrain<IWorkScheduleGrain>(workSchedule.Manager, Standards.FormatYearMonth(workSchedule.Year, workSchedule.Month).ToString(CultureInfo.InvariantCulture)).PutKernel(workSchedule);
        }

        #endregion
    }
}

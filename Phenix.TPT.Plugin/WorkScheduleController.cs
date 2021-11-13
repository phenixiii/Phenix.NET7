using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.SyncCollections;
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

        private DateTime GetDeadline()
        {
            DateTime now = DateTime.Now;
            return now.Day > 5 ? new DateTime(now.Year, now.Month, 5) : new DateTime(now.Year, now.Month, 5).AddMonths(-1);
        }

        /// <summary>
        /// 获取工作档期(如不存在则返回初始对象)
        /// </summary>
        /// <param name="manager">管理人员</param>
        /// <param name="pastMonths">往期月份数</param>
        /// <param name="newMonths">新生月份数</param>
        [Authorize]
        [HttpGet("all")]
        public IList<WorkSchedule> GetAll(long manager, short pastMonths, short newMonths)
        {
            SynchronizedList<WorkSchedule> result = new SynchronizedList<WorkSchedule>();
            List<Task> tasks = new List<Task>();
            DateTime deadline = GetDeadline();
            for (int i = -pastMonths; i < newMonths; i++)
            {
                short year = (short) (deadline.Month + i < 1 ? deadline.Year - 1 : deadline.Month + i > 12 ? deadline.Year + 1 : deadline.Year);
                short month = (short) (deadline.Month + i < 1 ? deadline.Month + i + 12 : deadline.Month + i > 12 ? deadline.Month + i - 12 : deadline.Month + i);
                tasks.Add(Task.Run(async () => { result.Add(await ClusterClient.Default.GetGrain<IWorkScheduleGrain>(manager, Standards.FormatYearMonth(year, month).ToString(CultureInfo.InvariantCulture)).FetchKernel(true)); }));
            }

            Task.WaitAll(tasks.ToArray());
            return result;
        }

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

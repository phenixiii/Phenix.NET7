using System;
using System.Collections.Generic;
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

        /// <summary>
        /// 获取工作档期(如不存在则返回初始对象)
        /// </summary>
        /// <param name="pastMonths">往期月份数</param>
        /// <param name="newMonths">新生月份数</param>
        [Authorize]
        [HttpGet("all")]
        public IDictionary<long, IList<WorkSchedule>> GetAll(short pastMonths, short newMonths)
        {
            SynchronizedDictionary<long, IList<WorkSchedule>> result = new SynchronizedDictionary<long, IList<WorkSchedule>>();
            List<Task> tasks = new List<Task>();
            DateTime firstDay = DateTime.Now.AddDays(1 - DateTime.Now.Day).Date;
            DateTime lastDay = firstDay.AddMonths(1).AddMilliseconds(-1);
            foreach (ProjectInfoS item in ProjectInfoS.FetchList(Database.Default,
                p => p.OriginateTime <= lastDay && (p.ClosedDate == null || p.ClosedDate >= firstDay)))
            {
                //项目经理
                long projectManager = item.ProjectManager;
                if (!result.ContainsKey(projectManager))
                {
                    result.Add(projectManager, null);
                    tasks.Add(Task.Run(async () =>
                    {
                        result[projectManager] = await ClusterClient.Default.GetGrain<IWorkScheduleGrain>(projectManager).FetchWorkSchedules(pastMonths, newMonths);
                    }));
                }
                //开发经理
                long developManager = item.DevelopManager;
                if (!result.ContainsKey(developManager))
                {
                    result.Add(developManager, null);
                    tasks.Add(Task.Run(async () =>
                    {
                        result[developManager] = await ClusterClient.Default.GetGrain<IWorkScheduleGrain>(developManager).FetchWorkSchedules(pastMonths, newMonths);
                    }));
                }
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
            return await ClusterClient.Default.GetGrain<IWorkScheduleGrain>(manager).FetchWorkSchedule(year, month);
        }

        /// <summary>
        /// 更新工作档期(如不存在则新增)
        /// </summary>
        [Authorize]
        [HttpPut]
        public async Task Put()
        {
            WorkSchedule workSchedule = await Request.ReadBodyAsync<WorkSchedule>();
            await ClusterClient.Default.GetGrain<IWorkScheduleGrain>(workSchedule.Manager).PutWorkSchedule(workSchedule);
        }

        #endregion
    }
}

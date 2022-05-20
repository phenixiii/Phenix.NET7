using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.SyncCollections;
using Phenix.TPT.Plugin.Business;

namespace Phenix.TPT.Plugin.WebApi
{
    /// <summary>
    /// 项目工作量控制器
    /// </summary>
    [Route(WebApiPaths.ProjectWorkloadPath)]
    [ApiController]
    public sealed class ProjectWorkloadController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 方法

        /// <summary>
        /// 获取worker-项目工作量(如不存在则返回初始对象)
        /// </summary>
        /// <param name="workers">打工人</param>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        [Authorize]
        [HttpGet("all")]
        public IDictionary<long, IList<ProjectWorkload>> GetAll(string workers, short year, short month)
        {
            SynchronizedDictionary<long, IList<ProjectWorkload>> result = new SynchronizedDictionary<long, IList<ProjectWorkload>>();
            List<Task> tasks = new List<Task>();
            foreach (string s in workers.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                long worker = Int64.Parse(s);
                tasks.Add(Task.Run(async () =>
                {
                    result.Add(worker, await ClusterClient.Default.GetGrain<IProjectWorkloadGrain>(worker, Standards.FormatYearMonth(year, month).ToString(CultureInfo.InvariantCulture)).GetProjectWorkloads());
                }));
            }

            Task.WaitAll(tasks.ToArray());
            return result;
        }

        /// <summary>
        /// 获取项目工作量总数
        /// </summary>
        /// <param name="projectId">项目ID</param>
        [Authorize]
        [HttpGet("total")]
        public async Task<int> GetTotal(long projectId)
        {
            return await ClusterClient.Default.GetGrain<IProjectGrain>(projectId).TotalProjectWorkload();
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

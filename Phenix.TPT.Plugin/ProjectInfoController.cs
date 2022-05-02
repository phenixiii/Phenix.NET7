using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Mapper.Expressions;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 项目资料控制器
    /// </summary>
    [Route(WebApiConfig.ProjectInfoPath)]
    [ApiController]
    public sealed class ProjectInfoController : Phenix.Net.Api.ControllerBase
    {
        #region 方法

        /// <summary>
        /// 获取当月已制单且月初还未关闭的项目资料
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        [Authorize]
        [HttpGet("all")]
        public IList<ProjectInfoS> GetAll(short year, short month)
        {
            DateTime firstDay = new DateTime(year, month, 1);
            DateTime lastDay = firstDay.AddMonths(1).AddMilliseconds(-1);
            return ProjectInfoS.FetchList(Database.Default,
                p => p.OriginateTime <= lastDay && (p.ClosedDate == null || p.ClosedDate >= firstDay),
                OrderBy.Descending<ProjectInfoS>(p => p.ContApproveDate).
                    Descending(p => p.UpdateTime).
                    Descending(p => p.OriginateTime));
        }

        /// <summary>
        /// 获取项目资料(如不存在则返回初始对象)
        /// </summary>
        /// <param name="id">项目ID</param>
        [Authorize]
        [HttpGet]
        public async Task<ProjectInfo> Get(long? id)
        {
            return await ClusterClient.Default.GetGrain<IProjectGrain>(id ?? Database.Default.Sequence.Value).FetchKernel(true);
        }

        /// <summary>
        /// 更新项目资料(如不存在则新增)
        /// </summary>
        [Authorize]
        [HttpPut]
        public async Task Put()
        {
            ProjectInfo projectInfo = await Request.ReadBodyAsync<ProjectInfo>();
            await ClusterClient.Default.GetGrain<IProjectGrain>(projectInfo.Id).PutKernel(projectInfo);
        }

        /// <summary>
        /// 关闭项目
        /// </summary>
        /// <param name="id">项目ID</param>
        /// <param name="closedDate">关闭日期</param>
        [Authorize]
        [HttpDelete]
        public async Task Close(long id, DateTime closedDate)
        {
            await ClusterClient.Default.GetGrain<IProjectGrain>(id).Close(closedDate);
        }

        #endregion
    }
}

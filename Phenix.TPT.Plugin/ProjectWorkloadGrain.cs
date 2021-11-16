using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security;
using System.Threading.Tasks;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.TPT.Business;
using Phenix.TPT.Business.Norm;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 项目工作量Grain
    /// key: Worker（PH7_User.US_ID）
    /// keyExtension: Standards.FormatYearMonth(year, month)
    /// </summary>
    public class ProjectWorkloadGrain : StreamGrainBase<string>, IProjectWorkloadGrain
    {
        #region 属性
        
        #region Stream

        /// <summary>
        /// StreamId
        /// </summary>
        protected override Guid StreamId
        {
            get { return StreamConfig.ProjectStreamId; }
        }

        /// <summary>
        /// (自己作为Observer)侦听的一组StreamNamespace
        /// </summary>
        protected override string[] ListenStreamNamespaces
        {
            get { return new string[] {Worker.ToString()}; }
        }

        #endregion

        /// <summary>
        /// 打工人
        /// </summary>
        protected long Worker
        {
            get { return PrimaryKeyLong; }
        }

        private DateTime? _yearMonth;

        /// <summary>
        /// 年月
        /// </summary>
        protected DateTime YearMonth
        {
            get { return _yearMonth ??= DateTime.Parse(PrimaryKeyExtension); }
        }

        private IDictionary<long, ProjectWorkload> _kernel;

        /// <summary>
        /// 项目ID-项目工作量
        /// </summary>
        protected IDictionary<long, ProjectWorkload> Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    //取出已填报过的ProjectWorkload
                    IDictionary<long, ProjectWorkload> result = ProjectWorkload.FetchKeyValues(Database,
                        p => p.PiId,
                        p => p.Worker == Worker && p.Year == YearMonth.Year && p.Month == YearMonth.Month);
                    //补漏有自己工作档期的ProjectWorkload初始化对象
                    foreach (ProjectWorkerV item in ProjectWorkerV.FetchList(Database,
                        p => p.Worker == Worker && p.Year == YearMonth.Year && p.Month == YearMonth.Month))
                        if (!result.ContainsKey(item.Id))
                            result.Add(item.Id, ProjectWorkload.New(Database,
                                ProjectWorkload.Set(p => p.Year, YearMonth.Year).
                                    Set(p => p.Month, YearMonth.Month).
                                    Set(p => p.Worker, Worker).
                                    Set(p => p.PiId, item.Id)));
                    //补漏有自己负责项目的ProjectWorkload初始化对象
                    DateTime lastDay = YearMonth.AddMonths(1).AddMilliseconds(-1);
                    foreach (ProjectInfo item in ProjectInfo.FetchList(Database,
                        p => p.OriginateTime <= lastDay && (p.ClosedDate == null || p.ClosedDate >= YearMonth) &&
                             (p.ProjectManager == Worker || p.DevelopManager == Worker || p.MaintenanceManager == Worker || p.SalesManager == Worker)))
                        if (!result.ContainsKey(item.Id))
                            result.Add(item.Id, ProjectWorkload.New(Database,
                                ProjectWorkload.Set(p => p.Year, YearMonth.Year).
                                    Set(p => p.Month, YearMonth.Month).
                                    Set(p => p.Worker, Worker).
                                    Set(p => p.PiId, item.Id)));
                    _kernel = result;
                }

                //不纠正与工作档期和负责项目存在冲突的已填报ProjectWorkload
                return _kernel;
            }
        }

        #endregion

        #region 方法

        #region Stream

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="token">StreamSequenceToken</param>
        protected override Task OnReceiving(string content, StreamSequenceToken token)
        {
            _kernel = null;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送消息刷新项目工作量
        /// </summary>
        /// <param name="receiver">侦听者</param>
        private Task SendEventForRefreshProjectWorkloads(long receiver)
        {
            return ClusterClient.GetStreamProvider().GetStream<string>(StreamConfig.ProjectStreamId, receiver.ToString()).OnNextAsync(receiver.ToString());
        }

        #endregion

        Task<IList<ProjectWorkload>> IProjectWorkloadGrain.GetProjectWorkloads()
        {
            IList<ProjectWorkload> result = new List<ProjectWorkload>(Kernel.Values);
            return Task.FromResult(result);
        }

        private DateTime GetDeadline()
        {
            DateTime now = DateTime.Now;
            return now.Day > 5 ? new DateTime(now.Year, now.Month, 5) : new DateTime(now.Year, now.Month, 5).AddMonths(-1);
        }

        async Task IProjectWorkloadGrain.PutProjectWorkload(ProjectWorkload source)
        {
            DateTime today = DateTime.Today;
            if (source.Year > today.Year || source.Year == today.Year && source.Month > today.Month)
                throw new ValidationException("未来不可得~");
            bool unlimited = await User.Identity.IsInRole(ProjectRoles.经营管理);
            if (!(unlimited || new DateTime(source.Year, source.Month, 1).AddDays(DateTime.Now.Day - 1) < GetDeadline()))
                throw new ValidationException("不允许修改已归档的工作量!");

            if (Kernel.TryGetValue(source.PiId, out ProjectWorkload projectWorkload))
            {
                ProjectInfo projectInfo = await ClusterClient.GetGrain<IProjectGrain>(source.PiId).FetchKernel();
                if (projectInfo == null)
                    throw new ValidationException("项目不存在!");
                if (!(unlimited || User.Identity.Id == source.Worker || User.Identity.Id == projectInfo.ProjectManager || User.Identity.Id == projectInfo.DevelopManager))
                    throw new SecurityException("管好自己的工作量就行啦!");

                //汇总当前已登记项目工作量
                int oldAllWorkload = 0;
                foreach (KeyValuePair<long, ProjectWorkload> kvp in Kernel)
                    oldAllWorkload = oldAllWorkload + kvp.Value.TotalWorkload;
                //不允许新汇总数超出当月工作日
                int overmuchWorkload = oldAllWorkload - projectWorkload.TotalWorkload + source.TotalWorkload - (await ClusterClient.GetGrain<IWorkdayGrain>(source.Year).GetWorkday(source.Month)).Days;
                if (overmuchWorkload > 0)
                    throw new ValidationException(String.Format("请将超出当月工作日的 {0} 天摊到所有参与项目上以尽可能体现真实的投入占比!", overmuchWorkload));
                //持久化
                if (projectWorkload.TotalWorkload == 0)
                {
                    if (source.TotalWorkload > 0)
                    {
                        source.InsertSelf();
                        Kernel[source.PiId] = source;
                    }
                }
                else if (projectWorkload.TotalWorkload > 0)
                {
                    if (source.TotalWorkload > 0)
                        projectWorkload.UpdateSelf(source);
                    else if (source.TotalWorkload == 0)
                    {
                        projectWorkload.DeleteSelf();
                        Kernel[source.PiId] = source;
                    }
                }

                //播报
                await SendEventForRefreshProjectWorkloads(source.PiId);
            }
            else
                throw new ValidationException("非项目组成员是填报不了工作量的!");
        }

        #endregion
    }
}

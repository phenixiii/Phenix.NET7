using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.TPT.Business;
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
            get { return StreamConfig.RefreshProjectWorkloadsStreamId; }
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
                                    Set(p => p.PiId, item.Id).
                                    Set(p => p.ProjectName, item.ProjectName)));
                    //补漏有自己负责项目的ProjectWorkload初始化对象
                    DateTime lastDay = YearMonth.Month < 12
                        ? new DateTime(YearMonth.Year, YearMonth.Month + 1, 1).AddMilliseconds(-1)
                        : new DateTime(YearMonth.Year + 1, 1, 1).AddMilliseconds(-1);
                    foreach (ProjectInfo item in ProjectInfo.FetchList(Database,
                        p => p.OriginateTime <= lastDay && (p.ClosedDate == null || p.ClosedDate >= YearMonth) &&
                             (p.ProjectManager == Worker || p.DevelopManager == Worker || p.MaintenanceManager == Worker)))
                        if (!result.ContainsKey(item.Id))
                            result.Add(item.Id, ProjectWorkload.New(Database,
                                ProjectWorkload.Set(p => p.Year, YearMonth.Year).
                                    Set(p => p.Month, YearMonth.Month).
                                    Set(p => p.Worker, Worker).
                                    Set(p => p.PiId, item.Id).
                                    Set(p => p.ProjectName, item.ProjectName)));
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

        #endregion

        Task<IList<ProjectWorkload>> IProjectWorkloadGrain.GetProjectWorkloads()
        {
            IList<ProjectWorkload> result = new List<ProjectWorkload>(Kernel.Values);
            return Task.FromResult(result);
        }

        async Task IProjectWorkloadGrain.PutProjectWorkload(ProjectWorkload source)
        {
            DateTime today = DateTime.Today;
            if (today.Year < source.Year || today.Year == source.Year && today.Month < source.Month)
                throw new ValidationException("未来不可得~");

            if (Kernel.TryGetValue(source.PiId, out ProjectWorkload projectWorkload))
            {
                //汇总当前已登记项目工作量
                int oldAllWorkload = 0;
                foreach (KeyValuePair<long, ProjectWorkload> kvp in Kernel)
                    oldAllWorkload = oldAllWorkload + kvp.Value.TotalWorkload;
                //不允许提交source后新的汇总数超出当月工作日
                int overmuchWorkload = oldAllWorkload - projectWorkload.TotalWorkload + source.TotalWorkload - (await ClusterClient.GetGrain<IWorkdayGrain>(source.Year).GetWorkday(source.Month)).Days;
                if (overmuchWorkload > 0)
                    throw new ValidationException(String.Format("提交的{0}年{1}月{2}项目工作量相比当月工作日余量多出{3}人天!", source.Year, source.Month, source.ProjectName, overmuchWorkload));
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
            }
            else
                throw new ValidationException(String.Format("提交的{0}年{1}月{2}项目工作量是无中生有的!",
                    source.Year, source.Month, source.Worker, source.ProjectName));
        }

        #endregion
    }
}

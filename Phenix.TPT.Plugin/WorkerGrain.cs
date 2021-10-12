using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Orleans.Streams;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.SyncCollections;
using Phenix.TPT.Business;
using Phenix.TPT.Contract;

namespace Phenix.TPT.Plugin
{
    /// <summary>
    /// 打工人Grain
    /// key: Worker（PH7_User.US_ID）
    /// </summary>
    public class WorkerGrain : StreamGrainBase<string>, IWorkerGrain
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

        private readonly SynchronizedDictionary<DateTime, IDictionary<long, ProjectWorkload>> _projectWorkloads =
            new SynchronizedDictionary<DateTime, IDictionary<long, ProjectWorkload>>() ;

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
            //刷新项目工作量
            _projectWorkloads.Clear();
            return Task.CompletedTask;
        }

        #endregion

        Task<IList<ProjectWorkload>> IWorkerGrain.GetProjectWorkloads(short year, short month)
        {
            if (month < 1 || month > 12)
                throw new ValidationException(String.Format("咱这可没{0}月份唉!", month));

            DateTime yearMonth = Standards.FormatYearMonth(year, month);
            IList<ProjectWorkload> result = new List<ProjectWorkload>(_projectWorkloads.GetValue(yearMonth, () =>
            {
                IDictionary<long, ProjectWorkload> value = ProjectWorkload.FetchKeyValues(Database,
                    p => p.PiId,
                    p => p.Worker == Worker && p.Year == year && p.Month == month);
                foreach (ProjectWorkerV item in ProjectWorkerV.FetchList(Database,
                    p => p.Worker == Worker && p.Year == year && p.Month == month))
                    if (!value.ContainsKey(item.Id))
                        value.Add(item.Id, ProjectWorkload.New(Database,
                            ProjectWorkload.Set(p => p.Year, year).
                                Set(p => p.Month, month).
                                Set(p => p.Worker, Worker).
                                Set(p => p.PiId, item.Id).
                                Set(p => p.ProjectName, item.ProjectName)));
                foreach (ProjectInfo item in ProjectInfo.FetchList(Database,
                    p => p.OriginateTime <= yearMonth &&
                         (p.ClosedDate == null || p.ClosedDate >= yearMonth) &&
                         (p.ProjectManager == Worker || p.DevelopManager == Worker || p.MaintenanceManager == Worker)))
                    if (!value.ContainsKey(item.Id))
                        value.Add(item.Id, ProjectWorkload.New(Database,
                            ProjectWorkload.Set(p => p.Year, year).
                                Set(p => p.Month, month).
                                Set(p => p.Worker, Worker).
                                Set(p => p.PiId, item.Id).
                                Set(p => p.ProjectName, item.ProjectName)));
                return value;
            }).Values);
            return Task.FromResult(result);
        }

        async Task IWorkerGrain.PutProjectWorkload(ProjectWorkload source)
        {
            DateTime today = DateTime.Today;
            if (source.Year < today.Year - 1 || source.Year > today.Year + 1)
                throw new ValidationException("提交的项目工作量仅限于前后一年内的!");
            if (source.Month < 1 || source.Month > 12)
                throw new ValidationException("提交的项目工作量月份仅限于1-12之间!");
            if (source.Worker != Worker)
                throw new ValidationException(String.Format("提交的项目工作量工作人员应该是{0}!", Worker));

            DateTime yearMonth = Standards.FormatYearMonth(source.Year, source.Month);
            if (_projectWorkloads.TryGetValue(yearMonth, out IDictionary<long, ProjectWorkload> projectWorkloads) &&
                projectWorkloads.TryGetValue(source.PiId, out ProjectWorkload projectWorkload))
            {
                int oldAllWorkload = 0;
                foreach (KeyValuePair<long, ProjectWorkload> kvp in projectWorkloads)
                    oldAllWorkload = oldAllWorkload + kvp.Value.Workload;
                int overmuchWorkload = oldAllWorkload - projectWorkload.Workload + source.Workload - (await ClusterClient.GetGrain<IWorkdayGrain>(source.Year).GetCurrentYearWorkday(source.Month)).Days;
                if (overmuchWorkload > 0)
                    throw new ValidationException(String.Format("提交的{0}年{1}月{2}项目工作量相比当月工作日余量多出{3}人天!", source.Year, source.Month, source.ProjectName, overmuchWorkload));
                
                if (projectWorkload.Workload == 0)
                {
                    if (source.Workload > 0)
                    {
                        source.InsertSelf();
                        projectWorkloads[source.PiId] = source;
                    }
                }
                else if (projectWorkload.Workload > 0)
                {
                    if (source.Workload > 0)
                        projectWorkload.UpdateSelf(source);
                    else if (source.Workload == 0)
                    {
                        projectWorkload.DeleteSelf();
                        projectWorkloads[source.PiId] = source;
                    }
                }
            }

            throw new InvalidOperationException(String.Format("应先获取{0}年{1}月{2}的{3}项目工作量记录再进行修改和提交!", 
                source.Year, source.Month, source.Worker, source.ProjectName));
        }

        #endregion
    }
}

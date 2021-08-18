using System;
using System.Collections.Generic;
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
    /// 工作人员Grain
    /// key: RootTeamsId
    /// keyExtension: Worker
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
            get { return new string[] { Standards.FormatCompoundKey(RootTeamsId, Worker) }; }
        }

        #endregion

        /// <summary>
        /// 所属公司ID
        /// </summary>
        protected long RootTeamsId
        {
            get { return PrimaryKeyLong; }
        }

        /// <summary>
        /// 工作人员
        /// </summary>
        protected string Worker
        {
            get { return PrimaryKeyExtension; }
        }

        private readonly SynchronizedDictionary<DateTime, IList<ProjectWorkload>> _projectWorkloads =
            new SynchronizedDictionary<DateTime, IList<ProjectWorkload>>() ;

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
            _projectWorkloads.Clear();
            return Task.CompletedTask;
        }

        #endregion

        Task<IList<ProjectWorkload>> IWorkerGrain.GetProjectWorkloads(short year, short month)
        {
            DateTime yearMonth = Standards.FormatYearMonth(year, month);
            return Task.FromResult(_projectWorkloads.GetValue(yearMonth, () =>
            {
                IDictionary<long, ProjectWorkload> result = ProjectWorkload.FetchKeyValues(Database,
                    p => p.PiId,
                    p => p.OriginateTeams == RootTeamsId && p.Worker == Worker && p.Year == year && p.Month == month);
                foreach (ProjectWorkerV item in ProjectWorkerV.FetchList(Database,
                    p => p.OriginateTeams == RootTeamsId && p.Worker == Worker && p.Year == year && p.Month == month))
                    if (!result.ContainsKey(item.Id))
                        result.Add(item.Id, ProjectWorkload.New(Database,
                            ProjectWorkload.Set(p => p.Year, year).
                                Set(p => p.Month, month).
                                Set(p => p.Worker, Worker).
                                Set(p => p.PiId, item.Id).
                                Set(p => p.ProjectName, item.ProjectName)));
                foreach (ProjectInfo item in ProjectInfo.FetchList(Database,
                    p => p.OriginateTeams == RootTeamsId && p.OriginateTime <= yearMonth &&
                         (p.ClosedDate == null || p.ClosedDate >= yearMonth) &&
                         (p.ProjectManager == Worker || p.DevelopManager == Worker || p.MaintenanceManager == Worker)))
                    if (!result.ContainsKey(item.Id))
                        result.Add(item.Id, ProjectWorkload.New(Database,
                            ProjectWorkload.Set(p => p.Year, year).
                                Set(p => p.Month, month).
                                Set(p => p.Worker, Worker).
                                Set(p => p.PiId, item.Id).
                                Set(p => p.ProjectName, item.ProjectName)));
                return new List<ProjectWorkload>(result.Values);
            }));
        }

        #endregion
    }
}

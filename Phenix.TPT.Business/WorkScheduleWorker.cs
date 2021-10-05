using System;
using System.ComponentModel.DataAnnotations;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

/* 
   builder:    phenixiii
   build time: 2021-08-16 21:01:29
   mapping to: PT7_WORK_SCHEDULE_WORKER 工作档期工作人员
*/

namespace Phenix.TPT.Business
{
    /// <summary>
    /// 工作档期工作人员
    /// </summary>
    [Serializable]
    public class WorkScheduleWorker : WorkScheduleWorker<WorkScheduleWorker>
    {
    }

    /// <summary>
    /// 工作档期工作人员
    /// </summary>
    [Serializable]
    [Display(Description = @"工作档期工作人员")]
    [Sheet("PT7_WORK_SCHEDULE_WORKER", PrimaryKeyName = "WW_ID")]
    public abstract class WorkScheduleWorker<T> : EntityBase<T>
        where T : WorkScheduleWorker<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected WorkScheduleWorker()
        {
            // used to fetch object, do not add code
        }

        /// <summary>
        /// 工作档期工作人员
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected WorkScheduleWorker(string dataSourceKey,
            long id, long wsId, long worker) 
            : base(dataSourceKey)
        {
            _id = id;
            _wsId = wsId;
            _worker = worker;
        }

        protected override void InitializeSelf()
        {
        }

        private long _id;
        /// <summary>
        /// 
        /// </summary>
        [Display(Description = @"")]
        [Column("WW_ID")]
        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private long _wsId;
        /// <summary>
        /// 工作档期
        /// </summary>
        [Display(Description = @"工作档期")]
        [Column("WW_WS_ID")]
        public long WsId
        {
            get { return _wsId; }
            set { _wsId = value; }
        }

        private long _worker;
        /// <summary>
        /// 工作人员
        /// </summary>
        [Display(Description = @"工作人员")]
        [Column("WW_WORKER")]
        public long Worker
        {
            get { return _worker; }
            set { _worker = value; }
        }

    }
}

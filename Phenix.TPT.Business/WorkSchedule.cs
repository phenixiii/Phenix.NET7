using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Phenix.Core.Data;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

/* 
   builder:    phenixiii
   build time: 2021-08-09 17:34:10
   mapping to: PT7_WORK_SCHEDULE 工作档期
   revision record: 
    1，属性Workers类型改成IList<string>
    2，添加YearMonth属性用于FetchKeyValues
*/

namespace Phenix.TPT.Business
{
    /// <summary>
    /// 工作档期
    /// </summary>
    [Serializable]
    public class WorkSchedule : WorkSchedule<WorkSchedule>
    {
        #region 属性

        /// <summary>
        /// 年月
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DateTime YearMonth
        {
            get { return Standards.FormatYearMonth(Year, Month); }
        }

        #endregion
    }

    /// <summary>
    /// 工作档期
    /// </summary>
    [Serializable]
    [Display(Description = @"工作档期")]
    [Sheet("PT7_WORK_SCHEDULE", PrimaryKeyName = "WS_ID")]
    public abstract class WorkSchedule<T> : EntityBase<T>
        where T : WorkSchedule<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected WorkSchedule()
        {
            // used to fetch object, do not add code
        }

        /// <summary>
        /// 工作档期
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected WorkSchedule(string dataSourceKey,
            long id, short year, short month, long manager, IList<long> workers, long originator, DateTime originateTime, long originateTeams, long updater, DateTime updateTime) 
            : base(dataSourceKey)
        {
            _id = id;
            _year = year;
            _month = month;
            _manager = manager;
            _workers = workers;
            _originator = originator;
            _originateTime = originateTime;
            _originateTeams = originateTeams;
            _updater = updater;
            _updateTime = updateTime;
        }

        protected override void InitializeSelf()
        {
        }

        private long _id;
        /// <summary>
        /// 
        /// </summary>
        [Display(Description = @"")]
        [Column("WS_ID")]
        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private short _year;
        /// <summary>
        /// 年
        /// </summary>
        [Display(Description = @"年")]
        [Column("WS_YEAR_WM")]
        public short Year
        {
            get { return _year; }
            set { _year = value; }
        }

        private short _month;
        /// <summary>
        /// 月
        /// </summary>
        [Display(Description = @"月")]
        [Column("WS_MONTH_WM")]
        public short Month
        {
            get { return _month; }
            set { _month = value; }
        }

        private long _manager;
        /// <summary>
        /// 管理人员
        /// </summary>
        [Display(Description = @"管理人员")]
        [Column("WS_MANAGER_WM")]
        public long Manager
        {
            get { return _manager; }
            set { _manager = value; }
        }

        //* 改写：string -> IList<long>
        private IList<long> _workers;
        /// <summary>
        /// 工作人员
        /// </summary>
        [Display(Description = @"工作人员")]
        [Column("WS_WORKERS")]
        public IList<long> Workers
        {
            get { return _workers; }
        }

        private long _originator;
        /// <summary>
        /// 制单人
        /// </summary>
        [Display(Description = @"制单人")]
        [Column("WS_ORIGINATOR")]
        public long Originator
        {
            get { return _originator; }
            set { _originator = value; }
        }

        private DateTime _originateTime;
        /// <summary>
        /// 制单时间
        /// </summary>
        [Display(Description = @"制单时间")]
        [Column("WS_ORIGINATE_TIME")]
        public DateTime OriginateTime
        {
            get { return _originateTime; }
            set { _originateTime = value; }
        }

        private long _originateTeams;
        /// <summary>
        /// 制单团体
        /// </summary>
        [Display(Description = @"制单团体")]
        [Column("WS_ORIGINATE_TEAMS")]
        public long OriginateTeams
        {
            get { return _originateTeams; }
            set { _originateTeams = value; }
        }

        private long _updater;
        /// <summary>
        /// 更新人
        /// </summary>
        [Display(Description = @"更新人")]
        [Column("WS_UPDATER")]
        public long Updater
        {
            get { return _updater; }
            set { _updater = value; }
        }

        private DateTime _updateTime;
        /// <summary>
        /// 更新时间
        /// </summary>
        [Display(Description = @"更新时间")]
        [Column("WS_UPDATE_TIME")]
        public DateTime UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Threading.Tasks;
using Phenix.Core.Data;
using Phenix.Core.Data.Common;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;
using Phenix.Core.Log;

namespace Demo
{
    /// <summary>
    /// 岗位资料
    /// </summary>
    [Serializable]
    public sealed class Position : EntityBase<Position>, IMemCachedEntity
    {
        private Position()
        {
            //for fetch
        }

        [Newtonsoft.Json.JsonConstructor]
        private Position(long id, string name, IList<string> roles)
        {
            _id = id;
            _name = name;
            _roles = roles != null ? new ReadOnlyCollection<string>(roles) : null;
        }

        #region 工厂

        /// <summary>
        /// 新增岗位资料
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="roles">角色数组</param>
        /// <returns>岗位资料</returns>
        public static Position New(string name, string[] roles)
        {
            InitializeTable();

            Position result = new Position(Sequence.Value, name, roles);
            Insert(result);
            return result;
        }
        
        /// <summary>
        /// 获取岗位资料
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="resetHoursLater">多少小时后重置(0为不重置、负数为即刻重置)</param>
        /// <returns>岗位资料</returns>
        public static Position Fetch(long id, int resetHoursLater = 8)
        {
            InitializeTable();

            return FetchMemCache<Position>(id, 8);
        }

        /// <summary>
        /// 获取全部岗位资料
        /// </summary>
        /// <returns>岗位资料清单</returns>
        public static IList<Position> FetchAll()
        {
            InitializeTable();

            return Select(Ascending(p => p.Name));
        }

        #endregion

        #region 属性

        private long _id;

        /// <summary>
        /// ID
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { Update(this, SetProperty(p => p.Name, value)); }
        }

        private ReadOnlyCollection<string> _roles;

        /// <summary>
        /// 角色清单
        /// </summary>
        public IList<string> Roles
        {
            get { return _roles; }
            set { Update(this, SetProperty(p => p.Roles, value)); }
        }

        [NonSerialized]
        private DateTime _invalidTime;

        /// <summary>
        /// 失效时间
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DateTime InvalidTime
        {
            get { return _invalidTime; }
            set { _invalidTime = value; }
        }

        #endregion

        #region 方法
        
        /// <summary>
        /// 删除岗位资料
        /// </summary>
        public void Delete()
        {
            if (DeleteRecord(CriteriaExpression.Where<Position>(p => p.Id == Id).NotExists<User>(p => p.PositionId)) == 0)
                throw new InvalidOperationException(String.Format("未能删除 {0} 岗位, 可能已被用在了用户管理上", Name));
            Task.Run(() => SaveRenovateLog(this, ExecuteAction.Delete));
        }

        private static void InitializeTable()
        {
            if (Sheet == null)
            {
                Database.Execute(InitializeTable);
                Database.ClearCache();
            }
        }

        private static void InitializeTable(DbConnection connection)
        {
            try
            {
                DbCommandHelper.ExecuteNonQuery(connection, @"
CREATE TABLE PH7_Position (
  PT_ID NUMERIC(15) NOT NULL,
  PT_Name VARCHAR(100) NOT NULL,
  PT_Roles VARCHAR(4000) NOT NULL,
  PRIMARY KEY(PT_ID),
  UNIQUE(PT_Name)
)", false);
            }
            catch (Exception ex)
            {
                Task.Run(() => EventLog.SaveLocal("Initialize PH7_Position Table", ex));
                throw;
            }
        }

        #endregion
    }
}
using System;
using Phenix.Business;

namespace Phenix.Services.Library.Security
{
    /// <summary>
    /// 岗位资料
    /// </summary>
    [Serializable]
    public class Position : Position<Position>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected Position()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected Position(string dataSourceKey, long id, string name, string[] roles,
            long originator, DateTime originateTime, long originateTeams, long updater, DateTime updateTime)
            : base(dataSourceKey, id, name, roles, originator, originateTime, originateTeams, updater, updateTime)
        {
        }
    }

    /// <summary>
    /// 岗位资料
    /// </summary>
    [Serializable]
    public abstract class Position<T> : EntityBase<T>
        where T : Position<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected Position()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected Position(string dataSourceKey, long id, string name, string[] roles, 
            long originator, DateTime originateTime, long originateTeams, long updater, DateTime updateTime)
            : base(dataSourceKey)
        {
            _id = id;
            _name = name;
            _roles = roles;
            _originator = originator;
            _originateTime = originateTime;
            _originateTeams = originateTeams;
            _updater = updater;
            _updateTime = updateTime;
        }

        #region 属性

        private long _id;

        /// <summary>
        /// 主键
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
        }

        private string[] _roles;

        /// <summary>
        /// 角色清单
        /// </summary>
        public string[] Roles
        {
            get { return _roles; }
        }

        private long _originator;

        /// <summary>
        /// 制单人
        /// </summary>
        public long Originator
        {
            get { return _originator; }
        }

        private DateTime _originateTime;

        /// <summary>
        /// 制单时间
        /// </summary>
        public DateTime OriginateTime
        {
            get { return _originateTime; }
        }

        private long _originateTeams;

        /// <summary>
        /// 制单团体
        /// </summary>
        public long OriginateTeams
        {
            get { return _originateTeams; }
        }

        private long _updater;

        /// <summary>
        /// 更新人
        /// </summary>
        public long Updater
        {
            get { return _updater; }
        }

        private DateTime _updateTime;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime
        {
            get { return _updateTime; }
        }

        #endregion
    }
}
using System;
using Phenix.Business;
using Phenix.Mapper.Expressions;

namespace Phenix.Services.Library.Security
{
    /// <summary>
    /// 团体资料
    /// </summary>
    [Serializable]
    public class Teams : Teams<Teams>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected Teams()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected Teams(string dataSourceKey, long id, long rootId, long parentId, Teams[] children, string name,
            long originator, DateTime originateTime, long originateTeams, long updater, DateTime updateTime)
            : base(dataSourceKey, id, rootId, parentId, children, name, originator, originateTime, originateTeams, updater, updateTime)
        {
        }
    }

    /// <summary>
    /// 团体资料
    /// </summary>
    [Serializable]
    public abstract class Teams<T> : TreeEntityBase<Teams>
        where T : Teams<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected Teams()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected Teams(string dataSourceKey, long id, long rootId, long parentId, Teams[] children, string name,
            long originator, DateTime originateTime, long originateTeams, long updater, DateTime updateTime)
            : base(dataSourceKey, id, rootId, parentId, children)
        {
            _name = name;
            _originator = originator;
            _originateTime = originateTime;
            _originateTeams = originateTeams;
            _updater = updater;
            _updateTime = updateTime;
        }

        #region 属性

        private string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
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

        #region 方法

        #region DeleteSelf

        /// <summary>
        /// 为删除自己追加条件表达式
        /// </summary>
        protected override CriteriaExpression AppendCriteriaForDeleteSelf(CriteriaExpression criteriaExpression)
        {
            return criteriaExpression.NotExists<User>(p => p.TeamsId);
        }

        #endregion

        #endregion
    }
}
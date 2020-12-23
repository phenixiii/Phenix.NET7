using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Phenix.Business;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Security;

namespace Demo
{
    /// <summary>
    /// 岗位资料
    /// </summary>
    [Serializable]
    public sealed class Position : BusinessBase<Position>
    {
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
            set { _name = value; }
        }

        private ReadOnlyCollection<string> _roles;

        /// <summary>
        /// 角色清单
        /// </summary>
        public IList<string> Roles
        {
            get { return _roles; }
            set { _roles = new ReadOnlyCollection<string>(value); }
        }

        #endregion

        #region 方法

        #region DeleteSelf

        /// <summary>
        /// 为删除自己追加条件表达式
        /// </summary>
        protected override CriteriaExpression AppendCriteriaForDeleteSelf(CriteriaExpression criteriaExpression)
        {
            return criteriaExpression.NotExists<User>(p => p.PositionId);
        }

        #endregion

        #endregion
    }
}
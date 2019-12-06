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
    public sealed class Position : CachedRootBusinessBase<Position>
    {
        private Position()
        {
            //禁止添加代码
        }

        [Newtonsoft.Json.JsonConstructor]
        private Position(long id, string name, IList<string> roles)
            : base(id)
        {
            _name = name;
            _roles = roles != null ? new ReadOnlyCollection<string>(roles) : null;
        }

        #region 属性

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
        /// 为DeleteSelf()函数执行时追加检查是否存在关联关系的外键条件表达式
        /// </summary>
        protected override CriteriaExpression AppendAssociationLambda(CriteriaExpression criteriaExpressionForDeleteSelf)
        {
            return criteriaExpressionForDeleteSelf.NotExists<User>(p => p.PositionId);
        }

        #endregion

        #endregion
    }
}
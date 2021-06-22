using System;
using System.Collections.Generic;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;

namespace Phenix.Services.Business.Security
{
    /// <summary>
    /// 团体资料
    /// </summary>
    [Serializable]
    public class Teams : Teams<Teams>
    {
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

        [Newtonsoft.Json.JsonConstructor]
        protected Teams(string dataSourceKey, long id, long rootId, long parentId, IList<Teams> children, string name)
            : base(dataSourceKey, id, rootId, parentId, children)
        {
            _name = name;
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
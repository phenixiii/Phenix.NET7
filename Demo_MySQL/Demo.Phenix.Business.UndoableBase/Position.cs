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
    public sealed class Position : UndoableBase<Position>
    {
        private Position()
        {
            //for fetch
        }
        
        [Newtonsoft.Json.JsonConstructor]
        private Position(bool? isNew, bool? isSelfDeleted, bool? isSelfDirty,
            IDictionary<string, object> oldPropertyValues, IDictionary<string, bool?> dirtyPropertyNames,
            long id, string name, IList<string> roles)
            : base(isNew, isSelfDeleted, isSelfDirty, oldPropertyValues, dirtyPropertyNames)
        {
            _id = id;
            _name = name;
            _roles = roles != null ? new ReadOnlyCollection<string>(roles) : null;
        }

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
        
        /// <summary>
        /// 删除岗位资料
        /// </summary>
        public void Delete()
        {
            if (DeleteRecord(CriteriaExpression.Where<Position>(p => p.Id == Id).NotExists<User>(p => p.PositionId)) == 0)
                throw new InvalidOperationException(String.Format("未能删除 {0} 岗位, 可能已被用在了用户管理上", Name));
        }

        #endregion
    }
}
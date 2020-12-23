using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Phenix.Client.DataModel;

namespace Phenix.Client.Security.Myself
{
    /// <summary>
    /// 岗位资料
    /// </summary>
    [Serializable]
    public class Position : DataBase<Position>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private Position()
        {
            //禁止添加代码
        }

        [Newtonsoft.Json.JsonConstructor]
        private Position(string dataSourceKey, long id, string name, IList<string> roles)
            : base(dataSourceKey)
        {
            _id = id;
            _name = name;
            _roles = roles != null ? new ReadOnlyCollection<string>(roles) : null;
        }

        #region 属性

        private readonly long _id;

        /// <summary>
        /// 主键
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        private readonly string _name;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        private readonly ReadOnlyCollection<string> _roles;

        /// <summary>
        /// 角色清单
        /// </summary>
        public IList<string> Roles
        {
            get { return _roles; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 确定是否属于指定的角色
        /// </summary>
        /// <param name="role">角色</param>
        /// <returns>属于指定的角色</returns>
        public bool IsInRole(string role)
        {
            if (String.IsNullOrEmpty(role))
                return true;
            bool foundRole = false;
            foreach (string s in role.Split('|', StringSplitOptions.RemoveEmptyEntries))
                if (!String.IsNullOrEmpty(s))
                {
                    if (_roles != null && _roles.Contains(s))
                        return true;
                    foundRole = true;
                }

            return !foundRole;
        }

        #endregion
    }
}
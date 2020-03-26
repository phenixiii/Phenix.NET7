using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Phenix.Core.Data.Model;

namespace Phenix.Client.Security
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
        private Position(long id, string name, IList<string> roles)
        {
            _id = id;
            _name = name;
            _roles = roles != null ? new ReadOnlyCollection<string>(roles) : null;
        }

        #region 属性

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
    }
}
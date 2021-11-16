using System;
using Phenix.Core.Data.Model;

namespace Phenix.Services.Business.Security
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
        protected Position(string dataSourceKey, long id, string name, string[] roles)
            : base(dataSourceKey, id, name, roles)
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
        protected Position(string dataSourceKey, long id, string name, string[] roles)
            : base(dataSourceKey)
        {
            _id = id;
            _name = name;
            _roles = roles;
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

        #endregion
    }
}
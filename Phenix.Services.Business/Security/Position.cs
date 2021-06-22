using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Phenix.Core.Data.Model;

namespace Phenix.Services.Business.Security
{
    /// <summary>
    /// 用户资料
    /// </summary>
    [Serializable]
    public class Position : Position<Position>
    {
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
        /// 初始化
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected Position(string dataSourceKey, long id, string name, IList<string> roles)
            : base(dataSourceKey)
        {
            _id = id;
            _name = name;
            _roles = roles != null ? new ReadOnlyCollection<string>(roles) : null;
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

        private ReadOnlyCollection<string> _roles;

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
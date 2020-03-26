using System;
using System.Collections.Generic;
using Phenix.Core.Data.Model;

namespace Phenix.Client.Security
{
    /// <summary>
    /// 团体资料
    /// </summary>
    [Serializable]
    public class Teams : TreeDataBase<Teams>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        private Teams()
        {
            //禁止添加代码
        }

        [Newtonsoft.Json.JsonConstructor]
        private Teams(long id, string name, long rootId, long parentId, IList<Teams> allChildren)
            : base(id, rootId, parentId, allChildren)
        {
            _name = name;
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

        #endregion
    }
}
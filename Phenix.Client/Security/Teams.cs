using System;
using System.Collections.Generic;

namespace Phenix.Client.Security
{
    /// <summary>
    /// 团体资料
    /// </summary>
    [Serializable]
    public class Teams
    {
        private Teams()
        {
            //禁止添加代码
        }

        [Newtonsoft.Json.JsonConstructor]
        private Teams(long id, string name, long rootId, long parentId, IList<Teams> allChildren)
        {
            _id = id;
            _name = name;
            _rootId = rootId;
            _parentId = parentId;
            if (id == rootId)
            {
                _root = this;
                if (allChildren != null)
                {
                    _allChildren = new List<Teams>(allChildren);
                    foreach (Teams item in allChildren)
                        item._root = this;
                }
                else
                    _allChildren = new List<Teams>();
            }
        }

        #region 属性

        private readonly long _id;

        /// <summary>
        /// ID
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

        private readonly long _rootId;

        /// <summary>
        /// 根ID
        /// </summary>
        public long RootId
        {
            get { return _rootId; }
        }

        private readonly long _parentId;

        /// <summary>
        /// 父ID
        /// </summary>
        public long ParentId
        {
            get { return _parentId; }
        }

        [NonSerialized]
        private Teams _root;

        /// <summary>
        /// 根对象
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Teams Root
        {
            get { return _root; }
        }

        [NonSerialized]
        private Teams _parent;

        /// <summary>
        /// 父对象
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Teams Parent
        {
            get
            {
                if (_id == _rootId)
                    return null;
                return _parent ?? (_parent = _root.FindInBranch(_parentId));
            }
        }

        private readonly List<Teams> _allChildren;

        /// <summary>
        /// 全部儿孙对象数组
        /// </summary>
        public IList<Teams> AllChildren
        {
            get {
                if (_id != _rootId)
                    return null;
                if (_allChildren != null)
                    return _allChildren.AsReadOnly();
                return null;
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 在枝杈中寻找
        /// </summary>
        /// <param name="id">ID</param>
        public Teams FindInBranch(long id)
        {
            foreach (Teams item in FetchChildren())
            {
                if (item._id == id)
                    return item;
                Teams result = item.FindInBranch(id);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// 获取子对象
        /// </summary>
        public IList<Teams> FetchChildren()
        {
            List<Teams> result = new List<Teams>();
            foreach (Teams item in _root._allChildren)
                if (item._parentId == _id)
                    result.Add(item);
            return result.AsReadOnly();
        }

        #endregion
    }
}
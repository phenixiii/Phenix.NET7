using System;
using System.Collections.Generic;

namespace Phenix.Client.DataModel
{
    /// <summary>
    /// 树实体基类
    /// </summary>
    [Serializable]
    public abstract class TreeDataBase<T> : DataBase<T>
        where T : TreeDataBase<T>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected TreeDataBase()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        protected TreeDataBase(string dataSourceKey, long id, long rootId, long parentId, IList<T> allChildren)
            : base(dataSourceKey)
        {
            _id = id;
            _rootId = rootId;
            _parentId = parentId;
            if (id == rootId)
            {
                _root = (T) this;
                if (allChildren != null)
                {
                    foreach (T item in allChildren)
                        item._root = (T) this;
                    _allChildren = new List<T>(allChildren);
                }
                else
                    _allChildren = new List<T>();
            }
        }

        #region 属性

        private long _id;

        /// <summary>
        /// 主键属性(映射表ID字段)
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        private long _rootId;

        /// <summary>
        /// 根ID(映射表Root_ID字段)
        /// </summary>
        public long RootId
        {
            get { return _rootId; }
        }

        [NonSerialized]
        private T _root;

        /// <summary>
        /// 根数据
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public T Root
        {
            get { return _root; }
        }

        private long _parentId;

        /// <summary>
        /// 父ID(映射表Parent_ID字段)
        /// </summary>
        public long ParentId
        {
            get { return _parentId; }
        }

        [NonSerialized]
        private T _parent;

        /// <summary>
        /// 主数据
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public T Parent
        {
            get
            {
                if (_parent == null)
                {
                    if (_root != null)
                        _parent = _root.FindInBranch(p => p.Id == _parentId);
                    else
                        return (T) this;
                }

                return _parent;
            }
            protected set
            {
                T parent = value != null ? _root.FindInBranch(p => p.Id == value.Id) : null;
                if (parent == null)
                    throw new ArgumentException("不允许切为新树或挂在其他树上", nameof(value));
                if (FindInBranch(p => p.Id == value.Id) != null)
                    throw new ArgumentException("不允许倒挂在自己或儿孙节点下", nameof(value));
                T oldParent = Parent;
                _parentId = parent._id;
                _parent = parent;
                parent._children = null;
                oldParent._children = null;
            }
        }

        [NonSerialized]
        private List<T> _children;

        /// <summary>
        /// 儿孙
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IList<T> Children
        {
            get
            {
                if (_children == null)
                {
                    if (_root != null)
                    {
                        List<T> result = new List<T>();
                        foreach (T item in _root._allChildren)
                            if (item._parentId == _id)
                            {
                                item._parent = (T) this;
                                result.Add(item);
                            }

                        _children = result;
                    }
                    else
                        return null;
                }

                return _children.AsReadOnly();
            }
        }

        private List<T> _allChildren;

        /// <summary>
        /// 全部儿孙对象
        /// </summary>
        public IList<T> AllChildren
        {
            get { return Id == _rootId ? _allChildren.AsReadOnly() : null; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="doCreate">调用本函数新增(子节点)</param>
        /// <param name="doInsert">调用本函数持久化(子节点,子节点ID)</param>
        /// <returns>子节点</returns>
        protected T AddChild(Func<T> doCreate, Func<T, long> doInsert)
        {
            if (doCreate == null)
                throw new ArgumentNullException(nameof(doCreate));
            if (doInsert == null)
                throw new ArgumentNullException(nameof(doInsert));

            T result = doCreate();
            if (result != null)
            {
                result._rootId = _rootId;
                result._parentId = _id;
                result._root = _root;
                result._parent = (T) this;
                result._children = null;
                result._allChildren = null;
                result._id = doInsert(result);
                _root._allChildren.Add(result);
                _children = null;
            }

            return result;
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="newParent">父节点</param>
        /// <param name="doUpdateSelf">调用本函数持久化(更新记录数)</param>
        protected void ChangeParent(T newParent, Action doUpdateSelf)
        {
            if (newParent == null)
                throw new ArgumentNullException(nameof(newParent));
            if (doUpdateSelf == null)
                throw new ArgumentNullException(nameof(doUpdateSelf));

            doUpdateSelf();
            Parent = newParent;
        }

        /// <summary>
        /// 在本枝杈中寻找
        /// </summary>
        /// <param name="criteria">匹配条件(节点,是否匹配)</param>
        /// <returns>节点</returns>
        public T FindInBranch(Func<T, bool> criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            return DoFindInBranch(criteria);
        }

        private T DoFindInBranch(Func<T, bool> criteria)
        {
            if (criteria((T) this))
                return (T) this;
            foreach (T item in _root._allChildren)
                if (item._parentId == _id)
                {
                    T result = item.DoFindInBranch(criteria);
                    if (result != null)
                        return result;
                }

            return null;
        }

        /// <summary>
        /// 删除本枝杈
        /// </summary>
        /// <param name="doDeleteBranch">调用本函数持久化(更新记录数)</param>
        /// <returns>更新记录数</returns>
        protected int DeleteBranch(Func<int> doDeleteBranch)
        {
            if (doDeleteBranch == null)
                throw new ArgumentNullException(nameof(doDeleteBranch));

            T parent = Parent;
            int result = doDeleteBranch();
            if (result > 0)
                DoDeleteBranch(new List<T>(_root._allChildren));
            parent._children = null;
            return result;
        }

        private void DoDeleteBranch(IList<T> allChildren)
        {
            foreach (T item in allChildren)
                if (item._parentId == _id)
                    item.DoDeleteBranch(allChildren);
            _root._allChildren.Remove((T) this);
        }

        #endregion
    }
}
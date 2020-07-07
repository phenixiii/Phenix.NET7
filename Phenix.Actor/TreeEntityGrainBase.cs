using System;
using System.Threading.Tasks;
using Orleans;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

namespace Phenix.Actor
{
    /// <summary>
    /// 树实体Grain基类
    /// </summary>
    public abstract class TreeEntityGrainBase<TKernel> : EntityGrainBase<TKernel>, ITreeEntityGrain<TKernel>
        where TKernel : TreeEntityBase<TKernel>
    {
        #region 属性

        private TKernel _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override TKernel Kernel
        {
            get { return _kernel ?? (_kernel = TreeEntityBase<TKernel>.FetchTree(Database, p => p.Id == Id)); }
            set { _kernel = value; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>更新记录数</returns>
        protected override int PatchKernel(params NameValue[] propertyValues)
        {
            return Kernel != null
                ? Kernel.UpdateSelf(propertyValues)
                : this is IGrainWithIntegerKey
                    ? TreeEntityBase<TKernel>.NewRoot(Database, Id, propertyValues).InsertSelf()
                    : TreeEntityBase<TKernel>.NewRoot(Database, propertyValues).InsertSelf();
        }

        private TKernel GetNode(long id, bool throwIfNotFound = true)
        {
            if (Kernel == null)
                throw new ArgumentException("需先有根节点", nameof(id));

            TKernel node = Kernel.FindInBranch(p => p.Id == id);
            if (node != null)
                return node;

            if (throwIfNotFound)
                throw new ArgumentException(String.Format("找不到ID为{0}的节点", id), nameof(id));
            return null;
        }

        /// <summary>
        /// 是否存在节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 ArgumentException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <returns>是否存在节点</returns>
        protected virtual bool HaveNode(long id, bool throwIfNotFound)
        {
            return GetNode(id, throwIfNotFound) != null;
        }

        Task<bool> ITreeEntityGrain<TKernel>.HaveNode(long id, bool throwIfNotFound)
        {
            return Task.FromResult(HaveNode(id, throwIfNotFound));
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        protected virtual long AddChildNode(long parentId, params NameValue[] propertyValues)
        {
            long result = Database.Sequence.Value;
            GetNode(parentId).AddChild(() => TreeEntityBase<TKernel>.New(Database, result, propertyValues));
            return result;
        }

        Task<long> ITreeEntityGrain<TKernel>.AddChildNode(long parentId, params NameValue[] propertyValues)
        {
            return Task.FromResult(AddChildNode(parentId, propertyValues));
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        /// <returns>更新记录数</returns>
        protected virtual int ChangeParentNode(long id, long parentId)
        {
            return GetNode(id).ChangeParent(GetNode(parentId));
        }

        Task<int> ITreeEntityGrain<TKernel>.ChangeParentNode(long id, long parentId)
        {
            return Task.FromResult(ChangeParentNode(id, parentId));
        }

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>更新记录数</returns>
        protected virtual int UpdateNode(long id, params NameValue[] propertyValues)
        {
            return GetNode(id).UpdateSelf(propertyValues);
        }

        Task<int> ITreeEntityGrain<TKernel>.UpdateNode(long id, params NameValue[] propertyValues)
        {
            return Task.FromResult(UpdateNode(id, propertyValues));
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        protected virtual int DeleteBranch(long id)
        {
            return GetNode(id).DeleteBranch();
        }

        Task<int> ITreeEntityGrain<TKernel>.DeleteBranch(long id)
        {
            return Task.FromResult(DeleteBranch(id));
        }

        #endregion
    }
}

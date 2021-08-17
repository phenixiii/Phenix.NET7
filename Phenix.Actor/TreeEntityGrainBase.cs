using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;

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
            get
            {
                if (_kernel == null)
                {
                    if (this is IGrainWithIntegerKey)
                        _kernel = TreeEntityBase<TKernel>.FetchTree(Database, p => p.Id == PrimaryKeyLong);
                }

                return _kernel;
            }
            set { _kernel = value; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected override Task PatchKernel(IDictionary<string, object> propertyValues)
        {
            if (Kernel != null)
                Kernel.UpdateSelf(propertyValues);
            else if (this is IGrainWithIntegerKey)
                TreeEntityBase<TKernel>.NewRoot(Database, PrimaryKeyLong, propertyValues).InsertSelf();
            else
                TreeEntityBase<TKernel>.NewRoot(Database, propertyValues).InsertSelf();
            return Task.CompletedTask;
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

        Task<bool> ITreeEntityGrain<TKernel>.HaveNode(long id, bool throwIfNotFound)
        {
            return Task.FromResult(GetNode(id, throwIfNotFound) != null);
        }

        Task<long> ITreeEntityGrain<TKernel>.AddChildNode(long parentId, params NameValue<TKernel>[] propertyValues)
        {
            return AddChildNode(parentId, NameValue<TKernel>.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        protected virtual Task<long> AddChildNode(long parentId, IDictionary<string, object> propertyValues)
        {
            TKernel childNode = GetNode(parentId).AddChild(() => TreeEntityBase<TKernel>.New(Database, propertyValues));
            return Task.FromResult(childNode.Id);
        }
        Task<long> ITreeEntityGrain<TKernel>.AddChildNode(long parentId, IDictionary<string, object> propertyValues)
        {
            return AddChildNode(parentId, propertyValues);
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        protected virtual Task ChangeParentNode(long id, long parentId)
        {
            GetNode(id).ChangeParent(GetNode(parentId));
            return Task.CompletedTask;
        }
        Task ITreeEntityGrain<TKernel>.ChangeParentNode(long id, long parentId)
        {
            return ChangeParentNode(id, parentId);
        }

        Task ITreeEntityGrain<TKernel>.UpdateNode(long id, params NameValue<TKernel>[] propertyValues)
        {
            return UpdateNode(id, NameValue<TKernel>.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected virtual Task UpdateNode(long id, IDictionary<string, object> propertyValues)
        {
            GetNode(id).UpdateSelf(propertyValues);
            return Task.CompletedTask;
        }
        Task ITreeEntityGrain<TKernel>.UpdateNode(long id, IDictionary<string, object> propertyValues)
        {
            return UpdateNode(id, propertyValues);
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        protected virtual Task<int> DeleteBranch(long id)
        {
            return Task.FromResult(GetNode(id).DeleteBranch());
        }
        Task<int> ITreeEntityGrain<TKernel>.DeleteBranch(long id)
        {
            return DeleteBranch(id);
        }

        #endregion
    }
}

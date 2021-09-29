using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.Core.Data;
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
        /// 获取根实体对象
        /// </summary>
        /// <param name="autoNew">不存在则新增</param>
        /// <returns>根实体对象</returns>
        protected override TKernel FetchKernel(bool autoNew = false)
        {
            return Kernel == null && autoNew
                ? this is IGrainWithIntegerKey
                    ? TreeEntityBase<TKernel>.NewRoot(Database, NameValue.Set<TKernel>(p => p.PrimaryKeyLong, PrimaryKeyLong))
                    : TreeEntityBase<TKernel>.NewRoot(Database)
                : Kernel;
        }

        /// <summary>
        /// 新增根实体对象并自动持久化
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <param name="throwIfFound">如果为 true, 则发现已存在时引发 InvalidOperationException，否则覆盖更新它</param>
        protected override void CreateKernel(IDictionary<string, object> propertyValues, bool throwIfFound = true)
        {
            if (Kernel != null)
                if (throwIfFound)
                    throw new System.ComponentModel.DataAnnotations.ValidationException("不允许重复新增!");
                else
                {
                    OnKernelOperating(ExecuteAction.Update, out object tag);
                    Kernel.UpdateSelf(propertyValues);
                    OnKernelOperated(ExecuteAction.Update, tag);
                }
            else if (this is IGrainWithIntegerKey)
            {
                OnKernelOperating(ExecuteAction.Insert, out object tag);
                TreeEntityBase<TKernel>.NewRoot(Database, propertyValues).InsertSelf(PrimaryKeyLong);
                OnKernelOperated(ExecuteAction.Insert, tag);
            }
            else
            {
                OnKernelOperating(ExecuteAction.Insert, out object tag);
                TreeEntityBase<TKernel>.NewRoot(Database, propertyValues).InsertSelf();
                OnKernelOperated(ExecuteAction.Insert, tag);
            }
        }

        /// <summary>
        /// 删除根实体对象
        /// </summary>
        protected override void DeleteKernel()
        {
            if (Kernel != null)
            {
                OnKernelOperating(ExecuteAction.Delete, out object tag);
                Kernel.DeleteBranch();
                OnKernelOperated(ExecuteAction.Delete, tag);
            }
        }

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 ArgumentException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <returns>节点</returns>
        protected virtual TKernel GetNode(long id, bool throwIfNotFound = true)
        {
            if (Kernel == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException("需先有根节点");

            TKernel node = Kernel.FindInBranch(p => p.Id == id);
            if (node != null)
                return node;

            if (throwIfNotFound)
                throw new InvalidOperationException(String.Format("找不到ID为{0}的节点", id));
            return null;
        }
        Task<TKernel> ITreeEntityGrain<TKernel>.GetNode(long id, bool throwIfNotFound)
        {
            return Task.FromResult(GetNode(id, throwIfNotFound));
        }

        Task<bool> ITreeEntityGrain<TKernel>.HaveNode(long id, bool throwIfNotFound)
        {
            return Task.FromResult(GetNode(id, throwIfNotFound) != null);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        protected virtual long AddChildNode(long parentId, IDictionary<string, object> propertyValues)
        {
            TKernel childNode = GetNode(parentId).AddChild(() => TreeEntityBase<TKernel>.New(Database, propertyValues));
            return childNode.Id;
        }
        Task<long> ITreeEntityGrain<TKernel>.AddChildNode(long parentId, IDictionary<string, object> propertyValues)
        {
            return Task.FromResult(AddChildNode(parentId, propertyValues));
        }
        Task<long> ITreeEntityGrain<TKernel>.AddChildNode(long parentId, params NameValue<TKernel>[] propertyValues)
        {
            return Task.FromResult(AddChildNode(parentId, NameValue<TKernel>.ToDictionary(propertyValues)));
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        protected virtual void ChangeParentNode(long id, long parentId)
        {
            GetNode(id).ChangeParent(GetNode(parentId));
        }
        Task ITreeEntityGrain<TKernel>.ChangeParentNode(long id, long parentId)
        {
            ChangeParentNode(id, parentId);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected virtual void UpdateNode(long id, IDictionary<string, object> propertyValues)
        {
            GetNode(id).UpdateSelf(propertyValues);
        }
        Task ITreeEntityGrain<TKernel>.UpdateNode(long id, IDictionary<string, object> propertyValues)
        {
            UpdateNode(id, propertyValues);
            return Task.CompletedTask;
        }
        Task ITreeEntityGrain<TKernel>.UpdateNode(long id, params NameValue<TKernel>[] propertyValues)
        {
            UpdateNode(id, NameValue<TKernel>.ToDictionary(propertyValues));
            return Task.CompletedTask;
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

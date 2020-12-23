using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phenix.Core.Actor;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;

namespace Phenix.Actor
{
    /// <summary>
    /// 实体Grain代理基类
    /// </summary>
    [Serializable]
    public abstract class TreeEntityGrainProxyBase<T, TKernel, TGrainInterface> : EntityGrainProxyBase<T, TKernel, TGrainInterface>, ITreeEntityGrainProxy<TKernel>
        where T : TreeEntityGrainProxyBase<T, TKernel, TGrainInterface>
        where TKernel : TreeEntityBase<TKernel>
        where TGrainInterface : ITreeEntityGrain<TKernel>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected TreeEntityGrainProxyBase()
        {
            //禁止添加代码
        }

        #region 方法

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public async Task<long> AddChildNodeAsync(long parentId, params NameValue[] propertyValues)
        {
            return await Grain.AddChildNode(parentId, propertyValues);
        }

        Task<long> ITreeEntityGrainProxy<TKernel>.AddChildNode(long parentId, params NameValue[] propertyValues)
        {
            return AddChildNodeAsync(parentId, propertyValues);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        public async Task<long> AddChildNodeAsync(long parentId, IDictionary<string, object> propertyValues)
        {
            return await Grain.AddChildNode(parentId, propertyValues);
        }

        Task<long> ITreeEntityGrainProxy<TKernel>.AddChildNode(long parentId, IDictionary<string, object> propertyValues)
        {
            return AddChildNodeAsync(parentId, propertyValues);
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        public async Task ChangeParentNodeAsync(long id, long parentId)
        {
            await Grain.ChangeParentNode(id, parentId);
        }

        Task ITreeEntityGrainProxy<TKernel>.ChangeParentNode(long id, long parentId)
        {
            return ChangeParentNodeAsync(id, parentId);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public async Task UpdateNodeAsync(long id, params NameValue[] propertyValues)
        {
            await Grain.UpdateNode(id, propertyValues);
        }

        Task ITreeEntityGrainProxy<TKernel>.UpdateNode(long id, params NameValue[] propertyValues)
        {
            return UpdateNodeAsync(id, propertyValues);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        public async Task UpdateNodeAsync(long id, IDictionary<string, object> propertyValues)
        {
            await Grain.UpdateNode(id, propertyValues);
        }

        Task ITreeEntityGrainProxy<TKernel>.UpdateNode(long id, IDictionary<string, object> propertyValues)
        {
            return UpdateNodeAsync(id, propertyValues);
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        public async Task<int> DeleteNodeAsync(long id)
        {
            return await Grain.DeleteBranch(id);
        }

        Task<int> ITreeEntityGrainProxy<TKernel>.DeleteNode(long id)
        {
            return DeleteNodeAsync(id);
        }

        #endregion
    }
}
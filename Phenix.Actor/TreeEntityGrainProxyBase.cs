using System;
using System.Threading.Tasks;
using Phenix.Core.Actor;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

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
        /// 更改父节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        /// <returns>更新记录数</returns>
        public async Task<int> ChangeParentNodeAsync(long id, long parentId)
        {
            return await Grain.ChangeParentNode(id, parentId);
        }

        Task<int> ITreeEntityGrainProxy<TKernel>.ChangeParentNode(long id, long parentId)
        {
            return ChangeParentNodeAsync(id, parentId);
        }

        /// <summary>
        /// 更新节点属性值
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>更新记录数</returns>
        public async Task<int> UpdateNodeAsync(long id, params NameValue[] propertyValues)
        {
            return await Grain.UpdateNode(id, propertyValues);
        }

        Task<int> ITreeEntityGrainProxy<TKernel>.UpdateNode(long id, params NameValue[] propertyValues)
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
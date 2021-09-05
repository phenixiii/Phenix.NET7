using System.Collections.Generic;
using System.Threading.Tasks;
using Phenix.Core.Data.Expressions;

namespace Phenix.Actor
{
    /// <summary>
    /// 树实体Grain接口
    /// </summary>
    public interface ITreeEntityGrain<TKernel> : IEntityGrain<TKernel>
    {
        #region 方法

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 ArgumentException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <returns>节点</returns>
        Task<TKernel> GetNode(long id, bool throwIfNotFound = true);

        /// <summary>
        /// 是否存在节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="throwIfNotFound">如果为 true, 则会在找不到信息时引发 ArgumentException; 如果为 false, 则在找不到信息时返回 null</param>
        /// <returns>是否存在节点</returns>
        Task<bool> HaveNode(long id, bool throwIfNotFound = true);

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        Task<long> AddChildNode(long parentId, IDictionary<string, object> propertyValues);

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        Task<long> AddChildNode(long parentId, params NameValue<TKernel>[] propertyValues);

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        Task ChangeParentNode(long id, long parentId);

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task UpdateNode(long id, IDictionary<string, object> propertyValues);

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task UpdateNode(long id, params NameValue<TKernel>[] propertyValues);

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="id">节点ID</param>
        Task<int> DeleteBranch(long id);

        #endregion
    }
}
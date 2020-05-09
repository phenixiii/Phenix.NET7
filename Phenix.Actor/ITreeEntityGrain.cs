using System.Threading.Tasks;
using Phenix.Core.Data.Schema;

namespace Phenix.Actor
{
    /// <summary>
    /// 树实体Grain接口
    /// </summary>
    public interface ITreeEntityGrain<TKernel> : IEntityGrain<TKernel>
    {
        #region 方法

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>子节点ID</returns>
        Task<long> AddChildNode(long parentId, params NameValue[] propertyValues);

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        /// <returns>更新记录数</returns>
        Task<int> ChangeParentNode(long id, long parentId);

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>更新记录数</returns>
        Task<int> UpdateNode(long id, params NameValue[] propertyValues);

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        Task<int> DeleteBranch(long id);

        #endregion
    }
}
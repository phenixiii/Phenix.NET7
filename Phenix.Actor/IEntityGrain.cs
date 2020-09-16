using System.Collections.Generic;
using System.Threading.Tasks;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

namespace Phenix.Actor
{
    /// <summary>
    /// 实体Grain接口
    /// </summary>
    public interface IEntityGrain<TKernel> : IGrain
        where TKernel : EntityBase<TKernel>
    {
        #region 方法

        /// <summary>
        /// 存在根实体对象
        /// </summary>
        /// <returns>是否存在</returns>
        Task<bool> ExistKernel();

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <returns>根实体对象</returns>
        Task<TKernel> FetchKernel();

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PatchKernel(TKernel source);

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task PatchKernel(params NameValue[] propertyValues);

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task PatchKernel(IDictionary<string, object> propertyValues);

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        Task<object> GetKernelProperty(string propertyName);

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        Task<TValue> GetKernelProperty<TValue>(string propertyName);

        #endregion
    }
}
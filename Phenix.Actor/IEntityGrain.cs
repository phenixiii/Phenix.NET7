using System.Collections.Generic;
using System.Threading.Tasks;
using Phenix.Core.Data.Expressions;

namespace Phenix.Actor
{
    /// <summary>
    /// 实体Grain接口
    /// </summary>
    public interface IEntityGrain<TKernel> : IEntityGrain 
    {
        #region 方法

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <param name="autoNew">不存在则新增</param>
        /// <returns>根实体对象</returns>
        Task<TKernel> FetchKernel(bool autoNew = false);

        /// <summary>
        /// 新增根实体对象并自动持久化
        /// </summary>
        /// <param name="source">数据源</param>
        Task CreateKernel(TKernel source);

        /// <summary>
        /// 新增根实体对象并自动持久化
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task CreateKernel(params NameValue<TKernel>[] propertyValues);

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="throwIfFound">如果为 true, 则发现已存在时引发 InvalidOperationException，否则覆盖更新它</param>
        /// <param name="throwIfNotOwn">如果为 true, 则发现制单人不是自己时引发 InvalidOperationException，否则覆盖更新它</param>
        Task PutKernel(TKernel source, bool throwIfFound = false, bool? throwIfNotOwn = null);

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task PutKernel(params NameValue<TKernel>[] propertyValues);

        /// <summary>
        /// 更新根实体对象(如不存在则引发 InvalidOperationException)
        /// </summary>
        /// <param name="source">数据源</param>
        Task PatchKernel(TKernel source);

        /// <summary>
        /// 更新根实体对象(如不存在则引发 InvalidOperationException)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task PatchKernel(params NameValue<TKernel>[] propertyValues);

        #endregion
    }

    /// <summary>
    /// 实体Grain接口
    /// </summary>
    public interface IEntityGrain : IGrain
    {
        #region 方法

        /// <summary>
        /// 存在根实体对象
        /// </summary>
        /// <returns>是否存在</returns>
        Task<bool> ExistKernel();

        /// <summary>
        /// 新增根实体对象并自动持久化
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task CreateKernel(IDictionary<string, object> propertyValues);

        /// <summary>
        /// 新增根实体对象并自动持久化
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task CreateKernel(params NameValue[] propertyValues);

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <param name="throwIfFound">如果为 true, 则发现已存在时引发 InvalidOperationException，否则覆盖更新它</param>
        /// <param name="throwIfNotOwn">如果为 true, 则发现制单人不是自己时引发 InvalidOperationException，否则覆盖更新它</param>
        Task PutKernel(IDictionary<string, object> propertyValues, bool throwIfFound = false, bool? throwIfNotOwn = null);

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="propertyValue">待更新属性值</param>
        /// <param name="throwIfFound">如果为 true, 则发现已存在时引发 InvalidOperationException，否则覆盖更新它</param>
        /// <param name="throwIfNotOwn">如果为 true, 则发现制单人不是自己时引发 InvalidOperationException，否则覆盖更新它</param>
        Task PutKernel(NameValue propertyValue, bool throwIfFound = false, bool? throwIfNotOwn = null);

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task PutKernel(params NameValue[] propertyValues);

        /// <summary>
        /// 更新根实体对象(如不存在则引发 InvalidOperationException)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task PatchKernel(IDictionary<string, object> propertyValues);

        /// <summary>
        /// 更新根实体对象(如不存在则引发 InvalidOperationException)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        Task PatchKernel(params NameValue[] propertyValues);

        /// <summary>
        /// 删除根实体对象
        /// </summary>
        Task DeleteKernel();

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        Task<object> GetKernelPropertyValue(string propertyName);

        #endregion
    }
}
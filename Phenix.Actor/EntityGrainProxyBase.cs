using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Phenix.Core.Actor;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;
using Phenix.Core.Reflection;

namespace Phenix.Actor
{
    /// <summary>
    /// 实体Grain代理基类
    /// </summary>
    [Serializable]
    public abstract class EntityGrainProxyBase<T, TKernel, TGrainInterface> : IEntityGrainProxy<TKernel>
        where T : EntityGrainProxyBase<T, TKernel, TGrainInterface>
        where TKernel : EntityBase<TKernel>
        where TGrainInterface : IEntityGrain<TKernel>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected EntityGrainProxyBase()
        {
            //禁止添加代码
        }

        #region 工厂

        /// <summary>
        /// 获取实体Grain代理
        /// </summary>
        /// <param name="grain">Grain接口</param>
        /// <returns>实体Grain代理</returns>
        public static T Fetch(TGrainInterface grain)
        {
            T result = DynamicFactory.Create<T>();
            result.Grain = grain;
            return result;
        }

        #endregion

        #region 属性

        [NonSerialized]
        private TGrainInterface _grain;

        /// <summary>
        /// Grain接口
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public TGrainInterface Grain
        {
            get { return _grain; }
            private set { _grain = value; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 存在根实体对象
        /// </summary>
        /// <returns>是否存在</returns>
        public async Task<bool> ExistKernelAsync()
        {
            return await Grain.ExistKernel();
        }

        Task<bool> IEntityGrainProxy<TKernel>.ExistKernel()
        {
            return ExistKernelAsync();
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <returns>根实体对象</returns>
        public async Task<TKernel> FetchKernelAsync()
        {
            return await Grain.FetchKernel();
        }

        Task<TKernel> IEntityGrainProxy<TKernel>.FetchKernel()
        {
            return FetchKernelAsync();
        }

        /// <summary>
        /// 更新根实体对象
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>是否成功</returns>
        public async Task<int> PatchKernelAsync(params NameValue[] propertyValues)
        {
            return await Grain.PatchKernel(propertyValues);
        }

        Task<int> IEntityGrainProxy<TKernel>.PatchKernel(params NameValue[] propertyValues)
        {
            return PatchKernelAsync(propertyValues);
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <returns>属性值</returns>
        public async Task<TValue> GetKernelPropertyAsync<TValue>(Expression<Func<TKernel, object>> propertyLambda)
        {
            return await Grain.GetKernelProperty<TValue>(Utilities.GetPropertyInfo<TKernel>(propertyLambda).Name);
        }

        Task<TValue> IEntityGrainProxy<TKernel>.GetKernelProperty<TValue>(Expression<Func<TKernel, object>> propertyLambda)
        {
            return GetKernelPropertyAsync<TValue>(propertyLambda);
        }

        /// <summary>
        /// 设置根实体对象属性值
        /// </summary>
        /// <param name="propertyLambda">含类属性的 lambda 表达式</param>
        /// <param name="newValue">新值</param>
        public NameValue SetKernelProperty(Expression<Func<TKernel, object>> propertyLambda, object newValue)
        {
            return NameValue.Set(propertyLambda, newValue);
        }

        #endregion
    }
}

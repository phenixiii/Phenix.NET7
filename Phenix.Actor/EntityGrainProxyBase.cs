﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Phenix.Core.Actor;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;
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
            T result = DynamicInstanceFactory.Create<T>();
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
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        public async Task PatchKernelAsync(TKernel source)
        {
            await Grain.PatchKernel(source);
        }

        Task IEntityGrainProxy<TKernel>.PatchKernel(TKernel source)
        {
            return PatchKernelAsync(source);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        public async Task PatchKernelAsync(params NameValue[] propertyValues)
        {
            await Grain.PatchKernel(propertyValues);
        }

        Task IEntityGrainProxy<TKernel>.PatchKernel(params NameValue[] propertyValues)
        {
            return PatchKernelAsync(propertyValues);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        public async Task PatchKernelAsync(IDictionary<string, object> propertyValues)
        {
            await Grain.PatchKernel(propertyValues);
        }

        Task IEntityGrainProxy<TKernel>.PatchKernel(IDictionary<string, object> propertyValues)
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

        #endregion
    }
}

﻿using Orleans;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;
using Phenix.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Phenix.Actor
{
    /// <summary>
    /// 实体Grain基类
    /// </summary>
    public abstract class EntityGrainBase<TKernel> : GrainBase, IEntityGrain<TKernel>
        where TKernel : EntityBase<TKernel>
    {
        #region 属性

        /// <summary>
        /// 数据库入口
        /// </summary>
        protected override Database Database
        {
            get { return _kernel != null ? _kernel.Database : base.Database; }
        }

        private TKernel _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected virtual TKernel Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    if (this is IGrainWithIntegerKey)
                        _kernel = EntityBase<TKernel>.FetchRoot(Database, p => p.PrimaryKeyLong == PrimaryKeyLong);
                }

                return _kernel;
            }
            set { _kernel = value; }
        }

        #endregion

        #region 方法

        Task<bool> IEntityGrain.ExistKernel()
        {
            return Task.FromResult(Kernel != null);
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <returns>根实体对象</returns>
        protected virtual Task<TKernel> FetchKernel()
        {
            return Task.FromResult(Kernel);
        }
        Task<TKernel> IEntityGrain<TKernel>.FetchKernel()
        {
            return FetchKernel();
        }


        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        protected virtual Task PutKernel(TKernel source)
        {
            if (Kernel != null)
                Kernel.UpdateSelf(source);
            else
            {
                source.InsertSelf();
                Kernel = source;
            }

            return Task.CompletedTask;
        }
        Task IEntityGrain<TKernel>.PutKernel(TKernel source)
        {
            return PutKernel(source);
        }

        Task IEntityGrain<TKernel>.PatchKernel(params NameValue<TKernel>[] propertyValues)
        {
            return PatchKernel(NameValue<TKernel>.ToDictionary(propertyValues));
        }

        Task IEntityGrain.PatchKernel(params NameValue[] propertyValues)
        {
            return PatchKernel(NameValue.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected virtual Task PatchKernel(IDictionary<string, object> propertyValues)
        {
            if (Kernel != null)
                Kernel.UpdateSelf(propertyValues);
            else if (this is IGrainWithIntegerKey)
                EntityBase<TKernel>.New(Database, PrimaryKeyLong, propertyValues).InsertSelf();
            else
                EntityBase<TKernel>.New(Database, propertyValues).InsertSelf();
            return Task.CompletedTask;
        }
        Task IEntityGrain.PatchKernel(IDictionary<string, object> propertyValues)
        {
            return PatchKernel(propertyValues);
        }

        /// <summary>
        /// 删除根实体对象
        /// </summary>
        protected virtual Task DeleteKernel()
        {
            if (Kernel != null)
                Kernel.DeleteSelf();
            return Task.CompletedTask;
        }
        Task IEntityGrain.DeleteKernel()
        {
            return DeleteKernel();
        }

        Task<object> IEntityGrain.GetKernelPropertyValue(string propertyName)
        {
            return Task.FromResult(GetKernelPropertyValue(propertyName));
        }

        Task<TValue> IEntityGrain.GetKernelPropertyValue<TValue>(string propertyName)
        {
            return Task.FromResult(Utilities.ChangeType<TValue>(GetKernelPropertyValue(propertyName)));
        }

        private object GetKernelPropertyValue(string propertyName)
        {
            if (Kernel == null)
                throw new InvalidOperationException("无法获取不存在的根实体对象的属性值!");

            return Utilities.GetMemberValue(Kernel, propertyName);
        }

        #endregion
    }
}
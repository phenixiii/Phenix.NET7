using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;
using Phenix.Core.Reflection;

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
        /// <param name="autoNew">不存在则新增</param>
        /// <returns>根实体对象</returns>
        protected virtual TKernel FetchKernel(bool autoNew = false)
        {
            return Kernel == null && autoNew
                ? this is IGrainWithIntegerKey
                    ? EntityBase<TKernel>.New(Database, NameValue.Set<TKernel>(p => p.PrimaryKeyLong, PrimaryKeyLong))
                    : EntityBase<TKernel>.New(Database)
                : Kernel;
        }
        Task<TKernel> IEntityGrain<TKernel>.FetchKernel(bool autoNew)
        {
            return Task.FromResult(FetchKernel(autoNew));
        }

        /// <summary>
        /// 操作根实体对象之前
        /// </summary>
        /// <param name="executeAction">执行动作</param>
        /// <param name="tag">标记</param>
        protected virtual void OnKernelOperating(ExecuteAction executeAction, out object tag)
        {
            tag = null;
        }

        /// <summary>
        /// 操作根实体对象之后
        /// </summary>
        /// <param name="executeAction">执行动作</param>
        /// <param name="tag">标记</param>
        protected virtual void OnKernelOperated(ExecuteAction executeAction, object tag)
        {
            //可处理OnKernelOperating传递来的tag
        }

        /// <summary>
        /// 新增根实体对象并自动持久化
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="throwIfFound">如果为 true, 则发现已存在时引发 InvalidOperationException，否则覆盖更新它</param>
        protected virtual void CreateKernel(TKernel source, bool throwIfFound = true)
        {
            if (Kernel != null)
                if (throwIfFound)
                    throw new System.ComponentModel.DataAnnotations.ValidationException("不允许重复新增!");
                else
                {
                    OnKernelOperating(ExecuteAction.Update, out object tag);
                    Kernel.UpdateSelf(source);
                    OnKernelOperated(ExecuteAction.Update, tag);
                }
            else if (this is IGrainWithIntegerKey)
            {
                OnKernelOperating(ExecuteAction.Insert, out object tag);
                source.InsertSelf(PrimaryKeyLong);
                OnKernelOperated(ExecuteAction.Insert, tag);
            }
            else
            {
                OnKernelOperating(ExecuteAction.Insert, out object tag);
                source.InsertSelf();
                OnKernelOperated(ExecuteAction.Insert, tag);
            }
        }
        Task IEntityGrain<TKernel>.CreateKernel(TKernel source, bool throwIfFound)
        {
            CreateKernel(source, throwIfFound);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 新增根实体对象并自动持久化
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <param name="throwIfFound">如果为 true, 则发现已存在时引发 InvalidOperationException，否则覆盖更新它</param>
        protected virtual void CreateKernel(IDictionary<string, object> propertyValues, bool throwIfFound = true)
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
                EntityBase<TKernel>.New(Database, propertyValues).InsertSelf(PrimaryKeyLong);
                OnKernelOperated(ExecuteAction.Insert, tag);
            }
            else
            {
                OnKernelOperating(ExecuteAction.Insert, out object tag);
                EntityBase<TKernel>.New(Database, propertyValues).InsertSelf();
                OnKernelOperated(ExecuteAction.Insert, tag);
            }
        }
        Task IEntityGrain.CreateKernel(IDictionary<string, object> propertyValues, bool throwIfFound)
        {
            CreateKernel(propertyValues, throwIfFound);
            return Task.CompletedTask;
        }
        Task IEntityGrain.CreateKernel(params NameValue[] propertyValues)
        {
            CreateKernel(NameValue.ToDictionary(propertyValues));
            return Task.CompletedTask;
        }
        Task IEntityGrain<TKernel>.CreateKernel(params NameValue<TKernel>[] propertyValues)
        {
            CreateKernel(NameValue<TKernel>.ToDictionary(propertyValues));
            return Task.CompletedTask;
        }

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="source">数据源</param>
        protected virtual void PutKernel(TKernel source)
        {
           CreateKernel(source, false);
        }
        Task IEntityGrain<TKernel>.PutKernel(TKernel source)
        {
            PutKernel(source);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected virtual void PutKernel(IDictionary<string, object> propertyValues)
        {
            CreateKernel(propertyValues, false);
        }
        Task IEntityGrain.PutKernel(IDictionary<string, object> propertyValues)
        {
            PutKernel(propertyValues);
            return Task.CompletedTask;
        }
        Task IEntityGrain.PutKernel(params NameValue[] propertyValues)
        {
            PutKernel(NameValue.ToDictionary(propertyValues));
            return Task.CompletedTask;
        }
        Task IEntityGrain<TKernel>.PutKernel(params NameValue<TKernel>[] propertyValues)
        {
            PutKernel(NameValue<TKernel>.ToDictionary(propertyValues));
            return Task.CompletedTask;
        }

        /// <summary>
        /// 更新根实体对象(如不存在则引发 InvalidOperationException)
        /// </summary>
        /// <param name="source">数据源</param>
        protected virtual void PatchKernel(TKernel source)
        {
            if (Kernel == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException("未发现待更新的对象!");

            OnKernelOperating(ExecuteAction.Update, out object tag);
            Kernel.UpdateSelf(source);
            OnKernelOperated(ExecuteAction.Update, tag);
        }
        Task IEntityGrain<TKernel>.PatchKernel(TKernel source)
        {
            PatchKernel(source);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 更新根实体对象(如不存在则引发 InvalidOperationException)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected virtual void PatchKernel(IDictionary<string, object> propertyValues)
        {
            if (Kernel == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException("未发现待更新的对象!");

            OnKernelOperating(ExecuteAction.Update, out object tag);
            Kernel.UpdateSelf(propertyValues);
            OnKernelOperated(ExecuteAction.Update, tag);
        }
        Task IEntityGrain.PatchKernel(IDictionary<string, object> propertyValues)
        {
            PatchKernel(propertyValues);
            return Task.CompletedTask;
        }
        Task IEntityGrain.PatchKernel(params NameValue[] propertyValues)
        {
            PatchKernel(NameValue.ToDictionary(propertyValues));
            return Task.CompletedTask;
        }
        Task IEntityGrain<TKernel>.PatchKernel(params NameValue<TKernel>[] propertyValues)
        {
            PatchKernel(NameValue<TKernel>.ToDictionary(propertyValues));
            return Task.CompletedTask;
        }

        /// <summary>
        /// 删除根实体对象
        /// </summary>
        protected virtual void DeleteKernel()
        {
            if (Kernel != null)
            {
                OnKernelOperating(ExecuteAction.Delete, out object tag);
                Kernel.DeleteSelf();
                OnKernelOperated(ExecuteAction.Delete, tag);
            }
        }
        Task IEntityGrain.DeleteKernel()
        {
            DeleteKernel();
            return Task.CompletedTask;
        }

        private object GetKernelPropertyValue(string propertyName)
        {
            if (Kernel == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException("无法获取不存在的根实体对象的属性值!");

            return Utilities.GetMemberValue(Kernel, propertyName);
        }
        Task<object> IEntityGrain.GetKernelPropertyValue(string propertyName)
        {
            return Task.FromResult(GetKernelPropertyValue(propertyName));
        }

        #endregion
    }
}
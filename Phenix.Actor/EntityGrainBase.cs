using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.Business;
using Phenix.Core.Data;
using Phenix.Mapper.Expressions;
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
        protected virtual Task<TKernel> FetchKernel(bool autoNew = false)
        {
            return Task.FromResult(Kernel == null && autoNew
                ? this is IGrainWithIntegerKey
                    ? EntityBase<TKernel>.New(Database, NameValue.Set<TKernel>(p => p.PrimaryKeyLong, PrimaryKeyLong))
                    : EntityBase<TKernel>.New(Database)
                : Kernel);
        }
        Task<TKernel> IEntityGrain<TKernel>.FetchKernel(bool autoNew)
        {
            return FetchKernel(autoNew);
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
        protected Task CreateKernel(TKernel source)
        {
            PutKernel(source, true);
            return Task.CompletedTask;
        }
        Task IEntityGrain<TKernel>.CreateKernel(TKernel source)
        {
            return CreateKernel(source);
        }

        /// <summary>
        /// 新增根实体对象并自动持久化
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected Task CreateKernel(IDictionary<string, object> propertyValues)
        {
            PutKernel(propertyValues, true);
            return Task.CompletedTask;
        }
        Task IEntityGrain.CreateKernel(IDictionary<string, object> propertyValues)
        {
            return CreateKernel(propertyValues);
        }
        Task IEntityGrain.CreateKernel(params NameValue[] propertyValues)
        {
            return CreateKernel(NameValue.ToDictionary(propertyValues));
        }
        Task IEntityGrain<TKernel>.CreateKernel(params NameValue<TKernel>[] propertyValues)
        {
            return CreateKernel(NameValue<TKernel>.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="throwIfFound">如果为 true, 则发现已存在时引发 InvalidOperationException，否则覆盖更新它</param>
        /// <param name="throwIfNotOwn">如果为 true, 则发现制单人不是自己时引发 InvalidOperationException，否则覆盖更新它</param>
        protected virtual Task PutKernel(TKernel source, bool throwIfFound = false, bool? throwIfNotOwn = null)
        {
            if (Kernel != null)
                if (throwIfFound && !throwIfNotOwn.HasValue ||
                    throwIfNotOwn.HasValue && throwIfNotOwn.Value && (long) Kernel.GetValue("Originator") != User.Identity.Id)
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

            return Task.CompletedTask;
        }
        Task IEntityGrain<TKernel>.PutKernel(TKernel source, bool throwIfFound, bool? throwIfNotOwn)
        {
            return PutKernel(source, throwIfFound, throwIfNotOwn);
        }

        /// <summary>
        /// 新增或更新根实体对象
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <param name="throwIfFound">如果为 true, 则发现已存在时引发 InvalidOperationException，否则覆盖更新它</param>
        /// <param name="throwIfNotOwn">如果为 true, 则发现制单人不是自己时引发 InvalidOperationException，否则覆盖更新它</param>
        protected virtual Task PutKernel(IDictionary<string, object> propertyValues, bool throwIfFound = false, bool? throwIfNotOwn = null)
        {
            if (Kernel != null)
                if (throwIfFound && !throwIfNotOwn.HasValue ||
                    throwIfNotOwn.HasValue && throwIfNotOwn.Value && (long) Kernel.GetValue("Originator") != User.Identity.Id)
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

            return Task.CompletedTask;
        }
        Task IEntityGrain.PutKernel(IDictionary<string, object> propertyValues, bool throwIfFound, bool? throwIfNotOwn)
        {
            return PutKernel(propertyValues, throwIfFound, throwIfNotOwn);
        }
        Task IEntityGrain.PutKernel(NameValue propertyValue, bool throwIfFound, bool? throwIfNotOwn)
        {
            return PutKernel(NameValue.ToDictionary(propertyValue), throwIfFound, throwIfNotOwn);
        }
        Task IEntityGrain.PutKernel(params NameValue[] propertyValues)
        {
            return PutKernel(NameValue.ToDictionary(propertyValues));
        }
        Task IEntityGrain<TKernel>.PutKernel(params NameValue<TKernel>[] propertyValues)
        {
            return PutKernel(NameValue<TKernel>.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 更新根实体对象(如不存在则引发 InvalidOperationException)
        /// </summary>
        /// <param name="source">数据源</param>
        protected virtual Task PatchKernel(TKernel source)
        {
            if (Kernel == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException("未发现待更新的对象!");

            OnKernelOperating(ExecuteAction.Update, out object tag);
            Kernel.UpdateSelf(source);
            OnKernelOperated(ExecuteAction.Update, tag);
            return Task.CompletedTask;
        }
        Task IEntityGrain<TKernel>.PatchKernel(TKernel source)
        {
            return PatchKernel(source);
        }

        /// <summary>
        /// 更新根实体对象(如不存在则引发 InvalidOperationException)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected virtual Task PatchKernel(IDictionary<string, object> propertyValues)
        {
            if (Kernel == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException("未发现待更新的对象!");

            OnKernelOperating(ExecuteAction.Update, out object tag);
            Kernel.UpdateSelf(propertyValues);
            OnKernelOperated(ExecuteAction.Update, tag);
            return Task.CompletedTask;
        }
        Task IEntityGrain.PatchKernel(IDictionary<string, object> propertyValues)
        {
            return PatchKernel(propertyValues);
        }
        Task IEntityGrain.PatchKernel(params NameValue[] propertyValues)
        {
            return PatchKernel(NameValue.ToDictionary(propertyValues));
        }
        Task IEntityGrain<TKernel>.PatchKernel(params NameValue<TKernel>[] propertyValues)
        {
            return PatchKernel(NameValue<TKernel>.ToDictionary(propertyValues));
        }

        /// <summary>
        /// 删除根实体对象
        /// </summary>
        protected virtual Task DeleteKernel()
        {
            if (Kernel != null)
            {
                OnKernelOperating(ExecuteAction.Delete, out object tag);
                Kernel.DeleteSelf();
                OnKernelOperated(ExecuteAction.Delete, tag);
            }
            
            return Task.CompletedTask;
        }
        Task IEntityGrain.DeleteKernel()
        {
            return DeleteKernel();
        }

        private Task<object> GetKernelPropertyValue(string propertyName)
        {
            if (Kernel == null)
                throw new System.ComponentModel.DataAnnotations.ValidationException("无法获取不存在的根实体对象的属性值!");

            return Task.FromResult(Utilities.GetMemberValue(Kernel, propertyName));
        }
        Task<object> IEntityGrain.GetKernelPropertyValue(string propertyName)
        {
            return GetKernelPropertyValue(propertyName);
        }

        #endregion
    }
}
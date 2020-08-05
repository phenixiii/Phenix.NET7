using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.Core.Data;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;
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
            get { return _kernel != null ? _kernel.Database : Database.Default; }
        }

        private TKernel _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected virtual TKernel Kernel
        {
            get { return _kernel ?? (_kernel = EntityBase<TKernel>.FetchRoot(Database, p => p.Id == Id)); }
            set { _kernel = value; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 存在根实体对象
        /// </summary>
        /// <returns>是否存在</returns>
        protected virtual bool ExistKernel()
        {
            return Kernel != null;
        }

        Task<bool> IEntityGrain<TKernel>.ExistKernel()
        {
            return Task.FromResult(ExistKernel());
        }

        /// <summary>
        /// 获取根实体对象
        /// </summary>
        /// <returns>根实体对象</returns>
        protected virtual TKernel FetchKernel()
        {
            return Kernel;
        }

        Task<TKernel> IEntityGrain<TKernel>.FetchKernel()
        {
            return Task.FromResult(FetchKernel());
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        protected virtual void PatchKernel(TKernel source)
        {
            if (Kernel != null)
                Kernel.UpdateSelf(source);
            else
            {
                source.InsertSelf();
                Kernel = source;
            }
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected virtual void PatchKernel(IDictionary<string, object> propertyValues)
        {
            if (Kernel != null)
                Kernel.UpdateSelf(propertyValues);
            else if (this is IGrainWithIntegerKey || this is IGrainWithIntegerCompoundKey)
                EntityBase<TKernel>.New(Database, Id, propertyValues).InsertSelf();
            else
                EntityBase<TKernel>.New(Database, propertyValues).InsertSelf();
        }

        Task IEntityGrain<TKernel>.PatchKernel(TKernel source)
        {
            PatchKernel(source);
            return Task.CompletedTask;
        }

        Task IEntityGrain<TKernel>.PatchKernel(params NameValue[] propertyValues)
        {
            PatchKernel(NameValue.ToDictionary(propertyValues));
            return Task.CompletedTask;
        }

        Task IEntityGrain<TKernel>.PatchKernel(IDictionary<string, object> propertyValues)
        {
            PatchKernel(propertyValues);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 获取根实体对象属性值
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性值</returns>
        protected virtual object GetKernelProperty(string propertyName)
        {
            return Utilities.GetMemberValue(Kernel, propertyName);
        }

        Task<object> IEntityGrain<TKernel>.GetKernelProperty(string propertyName)
        {
            return Task.FromResult(GetKernelProperty(propertyName));
        }

        Task<TValue> IEntityGrain<TKernel>.GetKernelProperty<TValue>(string propertyName)
        {
            return Task.FromResult((TValue) Utilities.ChangeType(GetKernelProperty(propertyName), typeof(TValue)));
        }

        #endregion
    }
}

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
    public abstract class EntityGrainBase<TKernel> : Grain, IEntityGrain<TKernel>
        where TKernel : EntityBase<TKernel>
    {
        #region 属性

        private long? _id;

        /// <summary>
        /// ID(映射表XX_ID字段)
        /// </summary>
        protected virtual long Id
        {
            get
            {
                if (!_id.HasValue)
                    _id = this.GetPrimaryKeyLong();
                return _id.Value;
            }
        }

        private TKernel _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected virtual TKernel Kernel
        {
            get { return _kernel ?? (_kernel = EntityBase<TKernel>.FetchRoot(Database.Default, p => p.Id == Id)); }
            set { _kernel = value; }
        }

        #endregion

        #region 方法

        Task<bool> IEntityGrain<TKernel>.ExistKernel()
        {
            return Task.FromResult(Kernel != null);
        }

        Task<TKernel> IEntityGrain<TKernel>.FetchKernel()
        {
            return Task.FromResult(Kernel);
        }

        Task<object> IEntityGrain<TKernel>.GetKernelProperty(string propertyName)
        {
            return Task.FromResult(Utilities.GetMemberValue(Kernel, propertyName));
        }

        Task<bool> IEntityGrain<TKernel>.UpdateKernelProperty(NameValue[] propertyValues)
        {
            return Task.FromResult(Kernel != null ? Kernel.UpdateSelf(propertyValues) == 1 : EntityBase<TKernel>.New(Database.Default, propertyValues).InsertSelf() == 1);
        }

        #endregion
    }
}

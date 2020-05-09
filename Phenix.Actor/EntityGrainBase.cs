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

        /// <summary>
        /// 数据库入口
        /// </summary>
        protected virtual Database Database
        {
            get { return _kernel != null ? _kernel.Database : Database.Default; }
        }

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
            get { return _kernel ?? (_kernel = EntityBase<TKernel>.FetchRoot(Database, p => p.Id == Id)); }
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

        Task<int> IEntityGrain<TKernel>.PatchKernel(params NameValue[] propertyValues)
        {
            return Task.FromResult(Kernel != null
                ? Kernel.UpdateSelf(propertyValues)
                : this is IGrainWithIntegerKey
                    ? EntityBase<TKernel>.New(Database, Id, propertyValues).InsertSelf()
                    : EntityBase<TKernel>.New(Database, propertyValues).InsertSelf());
        }

        Task<object> IEntityGrain<TKernel>.GetKernelProperty(string propertyName)
        {
            return Task.FromResult(Utilities.GetMemberValue(Kernel, propertyName));
        }

        #endregion
    }
}

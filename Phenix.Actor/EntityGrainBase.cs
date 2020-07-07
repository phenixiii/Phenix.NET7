using System;
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
        /// ID(默认映射表主键XX_ID字段或获取自复合主键Key以默认映射AB关联表外键XX_A_ID字段之一)
        /// </summary>
        protected virtual long Id
        {
            get
            {
                if (!_id.HasValue)
                    _id = GetPrimaryKeyLong(out _idExtension);
                return _id.Value;
            }
        }

        private long? _idExtension;

        /// <summary>
        /// ID扩展(如不为空说明获取自复合主键KeyExtension以默认映射AB关联表外键XX_B_ID字段之一)
        /// </summary>
        protected virtual long? IdExtension
        {
            get
            {
                if (!_idExtension.HasValue)
                    _id = GetPrimaryKeyLong(out _idExtension);
                return _idExtension;
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

        private long GetPrimaryKeyLong(out long? idExtension)
        {
            string keyExtension;
            long result = this.GetPrimaryKeyLong(out keyExtension);
            if (!String.IsNullOrEmpty(keyExtension))
                idExtension = Int64.Parse(keyExtension);
            else
                idExtension = null;
            return result;
        }

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
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>更新记录数</returns>
        protected virtual int PatchKernel(params NameValue[] propertyValues)
        {
            return Kernel != null
                ? Kernel.UpdateSelf(propertyValues)
                : this is IGrainWithIntegerKey
                    ? EntityBase<TKernel>.New(Database, Id, propertyValues).InsertSelf()
                    : EntityBase<TKernel>.New(Database, propertyValues).InsertSelf();
        }

        Task<int> IEntityGrain<TKernel>.PatchKernel(params NameValue[] propertyValues)
        {
            return Task.FromResult(PatchKernel(propertyValues));
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

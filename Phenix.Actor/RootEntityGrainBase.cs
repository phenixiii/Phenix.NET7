using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Phenix.Core.Data.Model;
using Phenix.Core.Reflection;

namespace Phenix.Actor
{
    /// <summary>
    /// 根实体Grain基类
    /// </summary>
    public abstract class RootEntityGrainBase<TRootEntityBase> : Grain, IRootEntityGrain
        where TRootEntityBase : RootEntityBase<TRootEntityBase>
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

        private TRootEntityBase _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected virtual TRootEntityBase Kernel
        {
            get { return _kernel ?? (_kernel = RootEntityBase<TRootEntityBase>.Fetch(p => p.Id == Id)); }
            private set { _kernel = value; }
        }

        #endregion

        #region 方法

        Task<string> IRootEntityGrain.SelectRecord()
        {
            return Task.FromResult(Utilities.JsonSerialize(Kernel));
        }

        Task<int> IRootEntityGrain.UpdateRecord(string propertyValues)
        {
            if (Kernel != null)
            {
                int result = RootEntityBase<TRootEntityBase>.Sheet.UpdateRecord(Utilities.JsonDeserialize<IDictionary<string, object>>(propertyValues));
                Kernel = null;
                return Task.FromResult(result);
            }

            return Task.FromResult(RootEntityBase<TRootEntityBase>.Sheet.InsertRecord(Utilities.JsonDeserialize<IDictionary<string, object>>(propertyValues)));
        }

        #endregion
    }
}

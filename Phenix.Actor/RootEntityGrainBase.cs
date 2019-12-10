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
    public abstract class RootEntityGrainBase<T> : Grain, IRootEntityGrain
        where T : RootEntityBase<T>
    {
        #region 属性

        private long? _id;

        /// <summary>
        /// ID(映射表XX_ID字段)
        /// </summary>
        protected long Id
        {
            get
            {
                if (!_id.HasValue)
                    _id = this.GetPrimaryKeyLong();
                return _id.Value;
            }
        }

        private T _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected T Kernel
        {
            get { return _kernel ?? (_kernel = RootEntityBase<T>.Fetch(p => p.Id == Id)); }
            private set { _kernel = value; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 获取记录(JSON格式)
        /// </summary>
        Task<string> IRootEntityGrain.SelectRecord()
        {
            return Task.FromResult(Utilities.JsonSerialize(Kernel));
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="propertyValues">待更新"属性名-属性值"键值队列(仅提交第一个属性映射的表)</param>
        /// <returns>更新记录数</returns>
        Task<int> IRootEntityGrain.UpdateRecord(string propertyValues)
        {
            if (Kernel != null)
            {
                int result = RootEntityBase<T>.Sheet.UpdateRecord(Utilities.JsonDeserialize<IDictionary<string, object>>(propertyValues));
                Kernel = null;
                return Task.FromResult(result);
            }

            return Task.FromResult(RootEntityBase<T>.Sheet.InsertRecord(Utilities.JsonDeserialize<IDictionary<string, object>>(propertyValues)));
        }

        #endregion
    }
}

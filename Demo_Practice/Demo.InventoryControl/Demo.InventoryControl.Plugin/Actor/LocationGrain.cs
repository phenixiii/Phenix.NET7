﻿using System.Threading.Tasks;
using Demo.InventoryControl.Plugin.Business;
using Orleans;
using Phenix.Actor;
using Phenix.Core.Data.Model;

namespace Demo.InventoryControl.Plugin.Actor
{
    /// <summary>
    /// 货架Grain
    /// </summary>
    public class LocationGrain : RootEntityGrainBase<IcLocation>, ILocationGrain
    {
        #region 属性

        private string _area;

        /// <summary>
        /// 库区
        /// </summary>
        public string Area
        {
            get { return _area ?? (_area = AppConfig.ExtractArea(this.GetPrimaryKeyString())); }
        }

        private string _alley;

        /// <summary>
        /// 巷道
        /// </summary>
        public string Alley
        {
            get { return _alley ?? (_alley = AppConfig.ExtractAlley(this.GetPrimaryKeyString())); }
        }

        private string _ordinal;

        /// <summary>
        /// 序号
        /// </summary>
        public string Ordinal
        {
            get { return _ordinal ?? (_ordinal = AppConfig.ExtractOrdinal(this.GetPrimaryKeyString())); }
        }

        /// <summary>
        /// ID(映射表XX_ID字段)
        /// </summary>
        protected override long Id
        {
            get { return Kernel.Id; }
        }

        private IcLocation _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override IcLocation Kernel
        {
            get
            {
                if (_kernel == null)
                {
                    _kernel = RootEntityBase<IcLocation>.Fetch(p => p.Area == Area && p.Alley == Alley && p.Ordinal == Ordinal);
                    if (_kernel == null)
                    {
                        _kernel = new IcLocation(Area, Alley, Ordinal);
                        _kernel.InsertSelf();
                    }
                }

                return _kernel;
            }
        }

        #endregion

        #region 方法

        Task ILocationGrain.Refresh()
        {
            Kernel.Refresh();
            return Task.CompletedTask;
        }

        #endregion
    }
}

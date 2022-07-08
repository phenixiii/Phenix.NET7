using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;
using Phenix.iPost.CSS.Plugin.Business.Property;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 船舶Grain
    /// key: vesselCode
    /// </summary>
    public class VesselGrain : GrainBase<VesselGrain>, IVesselGrain
    {
        public VesselGrain(ILogger<VesselGrain> logger, IEventBus eventBus)
            : base(logger, eventBus)
        {
        }

        #region 属性

        /// <summary>
        /// 船舶代码
        /// </summary>
        protected string VesselCode
        {
            get { return PrimaryKeyString; }
        }

        private Vessel _kernel;

        /// <summary>
        /// Vessel
        /// </summary>
        protected Vessel Kernel
        {
            get { return _kernel ??= new Vessel(VesselCode); }
        }

        #endregion

        #region 方法

        protected override Task ExecuteTimerAsync(object args)
        {
            throw new NotImplementedException();
        }

        #region Event

        public Task SetBayPlan(string voyage, IDictionary<int, BayPlanContainerProperty> bayPlan)
        {
            Kernel.SetBayPlan(voyage, bayPlan);
            return Task.CompletedTask;
        }

        Task IVesselGrain.OnBerth(string voyage, VesselAlongSideProperty alongSide)
        {
            Kernel.OnBerth(voyage, alongSide);
            return Task.CompletedTask;
        }

        Task IVesselGrain.OnDepart(string voyage)
        {
            Kernel.OnDepart(voyage);
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}
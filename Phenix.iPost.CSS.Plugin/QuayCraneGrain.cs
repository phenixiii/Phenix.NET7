using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 岸桥Grain
    /// key: machineId
    /// </summary>
    public class QuayCraneGrain : CraneGrain<QuayCraneGrain, QuayCrane>, IQuayCraneGrain
    {
        public QuayCraneGrain(ILogger<QuayCraneGrain> logger, IEventBus eventBus)
            : base(logger, eventBus)
        {
        }

        #region 属性

        private QuayCrane _kernel;

        /// <summary>
        /// Kernel
        /// </summary>
        protected override QuayCrane Kernel
        {
            get { return _kernel ??= new QuayCrane(MachineId); }
        }

        #endregion

        #region 方法

        protected override Task ExecuteTimerAsync(object args)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
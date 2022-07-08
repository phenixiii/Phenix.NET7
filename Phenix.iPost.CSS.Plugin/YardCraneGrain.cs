using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 场桥Grain
    /// key: machineId
    /// </summary>
    public class YardCraneGrain : CraneGrain<YardCraneGrain, YardCrane>, IYardCraneGrain
    {
        public YardCraneGrain(ILogger<YardCraneGrain> logger, IEventBus eventBus)
            : base(logger, eventBus)
        {
        }

        #region 属性

        private YardCrane _kernel;

        /// <summary>
        /// Kernel
        /// </summary>
        protected override YardCrane Kernel
        {
            get { return _kernel ??= new YardCrane(MachineId); }
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
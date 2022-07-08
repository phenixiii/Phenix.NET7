using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Business;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 吊车Grain
    /// key: machineId
    /// </summary>
    public abstract class CraneGrain<T, TKernel> : MachineGrain<T, TKernel>, ICraneGrain
        where T : CraneGrain<T, TKernel>
        where TKernel : Crane
    {
        /// <summary>
        /// 初始化
        /// </summary>
        protected CraneGrain(ILogger<T> logger, IEventBus eventBus)
            : base(logger, eventBus)
        {
        }

        #region 属性

        #endregion

        #region 方法

        #region Event

        Task ICraneGrain.OnAction(CraneAction action)
        {
            Kernel.OnAction(action);
            return Task.CompletedTask;
        }

        Task ICraneGrain.OnAction(CraneGrabAction grabAction, int hoistHeight)
        {
            Kernel.OnAction(grabAction, hoistHeight);
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}
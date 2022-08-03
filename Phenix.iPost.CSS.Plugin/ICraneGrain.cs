using System.Threading.Tasks;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 吊车Grain
    /// key: MachineId
    /// </summary>
    public interface ICraneGrain : IMachineGrain
    {
        #region Event

        /// <summary>
        /// 有动作
        /// </summary>
        /// <param name="craneAction">吊车动作</param>
        Task OnAction(CraneAction craneAction);

        /// <summary>
        /// 有动作
        /// </summary>
        /// <param name="grabAction">抓具动作</param>
        /// <param name="hoistHeight">起升高度cm</param>
        Task OnAction(CraneGrabAction grabAction, int hoistHeight);

        #endregion
    }
}

using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Security;
using Phenix.Services.Plugin.Actor.Security;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 岗位资料代理
    /// </summary>
    public sealed class PositionProxy : EntityGrainProxyBase<PositionProxy, Position, IPositionGrain>, IPositionProxy
    {
        #region 方法

        Task<bool> IPositionProxy.IsInRole(string role)
        {
            return Grain.IsInRole(role);
        }

        #endregion
    }
}

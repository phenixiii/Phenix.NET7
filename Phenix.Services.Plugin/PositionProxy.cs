using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Security;
using Phenix.Services.Plugin.Actor.Security;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 岗位资料代理
    /// </summary>
    public sealed class PositionProxy : IPositionProxy
    {
        #region 方法

        Task<bool> IPositionProxy.IsInRole(string role)
        {
            return Identity.CurrentIdentity.PositionId.HasValue
                ? ClusterClient.Default.GetGrain<IPositionGrain>(Identity.CurrentIdentity.PositionId.Value).IsInRole(role) 
                : Task.FromResult(false);
        }

        #endregion
    }
}

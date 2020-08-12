using Phenix.Actor;
using Phenix.Core.Security;
using Phenix.Services.Plugin.Actor.Security;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 用户资料代理工厂
    /// </summary>
    public class UserProxyFactory : IUserProxyFactory
    {
        IUserProxy IUserProxyFactory.FetchUserProxy(string name)
        {
            return UserProxy.Fetch(ClusterClient.Default.GetGrain<IUserGrain>(name));
        }

        ITeamsProxy IUserProxyFactory.FetchTeamsProxy(long id)
        {
            return TeamsProxy.Fetch(ClusterClient.Default.GetGrain<ITeamsGrain>(id));
        }

        IPositionProxy IUserProxyFactory.FetchPositionProxy(long id)
        {
            return PositionProxy.Fetch(ClusterClient.Default.GetGrain<IPositionGrain>(id));
        }
    }
}

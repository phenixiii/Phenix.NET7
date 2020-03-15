using Phenix.Actor;
using Phenix.Core.Security;
using Phenix.Services.Plugin.Actor;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 用户资料代理工厂
    /// </summary>
    public class UserProxyFactory : IUserProxyFactory
    {
        IUserProxy IUserProxyFactory.Fetch(string name)
        {
            return UserProxy.Fetch(ClusterClient.Default.GetGrain<IUserGrain>(name));
        }
    }
}

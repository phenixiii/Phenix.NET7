using Phenix.Core.Security;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 用户资料代理工厂
    /// </summary>
    public class UserProxyFactory : IUserProxyFactory
    {
        IUserProxy IUserProxyFactory.FetchUserProxy(string companyName, string userName)
        {
            return new UserProxy();
        }

        IPositionProxy IUserProxyFactory.FetchPositionProxy(long id)
        {
            return new PositionProxy();
        }
    }
}

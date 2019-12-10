using Phenix.Core.Security;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 用户资料工厂
    /// </summary>
    public class UserFactory : IUserFactory
    {
        public IUser Fetch(string name)
        {
            return User.FetchAsync(p => p.Name == name).Result;
        }

        public IUser New(string name, string phone, string eMail, string regAlias, string requestAddress, out string initialPassword, out string dynamicPassword)
        {
            return User.New(name, phone, eMail, regAlias, requestAddress, out initialPassword, out dynamicPassword);
        }

        public IUser New(string name, string password)
        {
            return User.New(name, password);
        }
    }
}

using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Reflection;
using Phenix.Core.Security;
using Phenix.Services.Plugin.Actor;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 用户资料代理
    /// </summary>
    public sealed class UserProxy : EntityGrainProxyBase<UserProxy, User, IUserGrain>, IUserProxy
    {
        #region 方法

        Task<string> IUserProxy.CheckIn(string name, string phone, string eMail, string regAlias, string requestAddress)
        {
            return Grain.CheckIn(name, phone, eMail, regAlias, requestAddress);
        }

        Task<bool> IUserProxy.IsValidLogon(string timestamp, string signature, string requestAddress, bool throwIfNotConform)
        {
            return Grain.IsValidLogon(timestamp, signature, requestAddress, throwIfNotConform);
        }

        Task<bool> IUserProxy.IsValid(string timestamp, string signature, string requestAddress, bool throwIfNotConform)
        {
            return Grain.IsValid(timestamp, signature, requestAddress, throwIfNotConform);
        }

        Task IUserProxy.Logon(string tag)
        {
            return Grain.Logon(tag);
        }

        Task<string> IUserProxy.Encrypt(object data)
        {
            return Grain.Encrypt(data is string ds ? ds : Utilities.JsonSerialize(data));
        }

        Task<string> IUserProxy.Decrypt(string cipherText)
        {
            return Grain.Decrypt(cipherText);
        }

        Task<bool> IUserProxy.ChangePassword(string newPassword, bool throwIfNotConform)
        {
            return Grain.ChangePassword(newPassword, throwIfNotConform);
        }

        Task<long> IUserProxy.PatchRootTeams(string name)
        {
            return Grain.PatchRootTeams(name);
        }

        #endregion
    }
}
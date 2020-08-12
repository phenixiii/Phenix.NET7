using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Reflection;
using Phenix.Core.Security;
using Phenix.Services.Plugin.Actor.Security;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 用户资料代理
    /// </summary>
    public sealed class UserProxy : EntityGrainProxyBase<UserProxy, User, IUserGrain>, IUserProxy
    {
        #region 方法

        Task<bool> IUserProxy.IsValidLogon(string timestamp, string signature, string tag, string requestAddress, bool throwIfNotConform)
        {
            return Grain.IsValidLogon(timestamp, signature, tag, requestAddress, throwIfNotConform);
        }

        Task<bool> IUserProxy.IsValid(string timestamp, string signature, string requestAddress, bool throwIfNotConform)
        {
            return Grain.IsValid(timestamp, signature, requestAddress, throwIfNotConform);
        }

        Task<string> IUserProxy.Encrypt(object data)
        {
            return Grain.Encrypt(data is string ds ? ds : Utilities.JsonSerialize(data));
        }

        Task<string> IUserProxy.Decrypt(string cipherText)
        {
            return Grain.Decrypt(cipherText);
        }

        #endregion
    }
}
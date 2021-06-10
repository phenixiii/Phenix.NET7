using System;
using System.Linq.Expressions;
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
    public sealed class UserProxy : IUserProxy
    {
        #region 方法

        Task<bool> IUserProxy.IsValidLogon(string companyName, string userName, string timestamp, string signature, string tag, string requestAddress, string requestSession, bool throwIfNotConform)
        {
            return ClusterClient.Default.GetGrain<IUserGrain>(Identity.FormatPrimaryKey(companyName, userName)).IsValidLogon(timestamp, signature, tag, requestAddress, requestSession, throwIfNotConform);
        }

        Task<bool> IUserProxy.IsValid(string companyName, string userName, string timestamp, string signature, string requestAddress, string requestSession, bool throwIfNotConform)
        {
            return ClusterClient.Default.GetGrain<IUserGrain>(Identity.FormatPrimaryKey(companyName, userName)).IsValid(timestamp, signature, requestAddress, requestSession, throwIfNotConform);
        }

        Task<string> IUserProxy.Encrypt(object data)
        {
            return ClusterClient.Default.GetGrain<IUserGrain>(Identity.CurrentIdentity.PrimaryKey).Encrypt(data is string ds ? ds : Utilities.JsonSerialize(data));
        }

        Task<string> IUserProxy.Decrypt(string cipherText)
        {
            return ClusterClient.Default.GetGrain<IUserGrain>(Identity.CurrentIdentity.PrimaryKey).Decrypt(cipherText);
        }

        Task<TValue> IUserProxy.GetKernelProperty<TValue>(Expression<Func<User, TValue>> propertyLambda)
        {
            return ClusterClient.Default.GetGrain<IUserGrain>(Identity.CurrentIdentity.PrimaryKey).GetKernelProperty<TValue>(Utilities.GetPropertyInfo<User>(propertyLambda).Name);
        }

        #endregion
    }
}
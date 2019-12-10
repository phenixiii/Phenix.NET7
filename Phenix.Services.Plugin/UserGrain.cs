using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Security.Auth;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 用户资料Grain
    /// </summary>
    public class UserGrain : RootEntityGrainBase<Phenix.Core.Security.User>, IUserGrain
    {
        #region 方法

        Task<string> IUserGrain.Encrypt(string sourceText)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.Encrypt(sourceText));
        }

        Task<string> IUserGrain.Decrypt(string cipherText)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.Decrypt(cipherText));
        }

        Task<bool> IUserGrain.IsValidLogon(string timestamp, string signature, string requestAddress, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.IsValidLogon(timestamp, signature, requestAddress, throwIfNotConform));
        }

        Task<bool> IUserGrain.IsValid(string timestamp, string signature, string requestAddress, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.IsValid(timestamp, signature, requestAddress, throwIfNotConform));
        }

        Task<bool> IUserGrain.ChangePassword(string newPassword, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.ChangePassword(newPassword, throwIfNotConform));
        }

        Task<string> IUserGrain.ApplyDynamicPassword(string requestAddress, bool throwIfNotConform)
        {
            if (Kernel == null)
                throw new UserNotFoundException();

            return Task.FromResult(Kernel.ApplyDynamicPassword(requestAddress, throwIfNotConform));
        }

        #endregion
    }
}
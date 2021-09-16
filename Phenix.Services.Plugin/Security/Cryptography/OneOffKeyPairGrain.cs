using System;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Security.Cryptography;
using Phenix.Core.SyncCollections;
using Phenix.Services.Contract.Security.Cryptography;

namespace Phenix.Services.Plugin.Security.Cryptography
{
    /// <summary>
    /// 一次性公钥私钥对Grain
    /// key: DiscardIntervalSeconds
    /// keyExtension: Name
    /// </summary>
    public class OneOffKeyPairGrain : GrainBase, IOneOffKeyPairGrain
    {
        #region 属性

        /// <summary>
        /// 丢弃间隔(秒)
        /// </summary>
        protected long DiscardIntervalSeconds
        {
            get { return PrimaryKeyLong > 0 ? PrimaryKeyLong : 60; }
        }

        /// <summary>
        /// 名称
        /// </summary>
        protected string Name
        {
            get { return PrimaryKeyExtension; }
        }

        private CachedObject<KeyPair> _keyPair;

        #endregion

        #region 方法

        Task<string> IOneOffKeyPairGrain.GetPublicKey()
        {
            KeyPair keyPair = RSACryptoTextProvider.CreateKeyPair();
            _keyPair = new CachedObject<KeyPair>(keyPair, DateTime.Now.AddSeconds(DiscardIntervalSeconds));
            return Task.FromResult(keyPair.PublicKey);
        }

        Task<string> IOneOffKeyPairGrain.Decrypt(string cipherText, bool fOAEP)
        {
            if (_keyPair == null)
                throw new InvalidOperationException("需先获取公钥才能解密!");
            if (_keyPair.IsInvalid)
                throw new InvalidOperationException("需重新获取公钥才能解密!");

            return Task.FromResult(RSACryptoTextProvider.Decrypt(_keyPair.Value.PrivateKey, cipherText, fOAEP));
        }

        #endregion
    }
}

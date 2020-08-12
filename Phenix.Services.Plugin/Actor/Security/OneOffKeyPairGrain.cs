using System;
using System.Threading.Tasks;
using Orleans;
using Phenix.Core.Security.Auth;
using Phenix.Core.Security.Cryptography;
using Phenix.Core.SyncCollections;

namespace Phenix.Services.Plugin.Actor.Security
{
    /// <summary>
    /// 一次性公钥私钥对Grain
    /// </summary>
    public class OneOffKeyPairGrain : Grain, IOneOffKeyPairGrain
    {
        #region 属性

        private long? _discardIntervalSeconds;

        /// <summary>
        /// 丢弃间隔(秒)
        /// </summary>
        protected long DiscardIntervalSeconds
        {
            get
            {
                if (!_discardIntervalSeconds.HasValue)
                    _discardIntervalSeconds = this.GetPrimaryKeyLong();
                return _discardIntervalSeconds.Value;
            }
        }

        private static readonly SynchronizedDictionary<string, KeyPair> _cache = new SynchronizedDictionary<string, KeyPair>(StringComparer.Ordinal);

        #endregion

        #region 方法

        Task<string> IOneOffKeyPairGrain.GetPublicKey(string name)
        {
            KeyPair keyPair = RSACryptoTextProvider.CreateKeyPair();
            keyPair.InvalidTime = DateTime.Now.AddSeconds(DiscardIntervalSeconds);
            _cache[name] = keyPair;
            return Task.FromResult(keyPair.PublicKey);
        }

        Task<string> IOneOffKeyPairGrain.Decrypt(string name, string cipherText, bool fOAEP)
        {
            if (_cache.TryGetValue(name, out KeyPair keyPair))
            {
                if (keyPair.InvalidTime < DateTime.Now)
                    throw new OvertimeRequestException();
                string result = RSACryptoTextProvider.Decrypt(keyPair.PrivateKey, cipherText, fOAEP);
                _cache.Remove(name);
                return Task.FromResult(result);
            }

            return null;
        }

        #endregion
    }
}

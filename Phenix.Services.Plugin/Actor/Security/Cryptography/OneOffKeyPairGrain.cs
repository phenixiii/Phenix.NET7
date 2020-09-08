using System;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Security.Cryptography;
using Phenix.Core.SyncCollections;

namespace Phenix.Services.Plugin.Actor.Security.Cryptography
{
    /// <summary>
    /// 一次性公钥私钥对Grain
    /// </summary>
    public class OneOffKeyPairGrain : GrainBase, IOneOffKeyPairGrain
    {
        #region 属性

        /// <summary>
        /// 丢弃间隔(秒)
        /// </summary>
        protected long DiscardIntervalSeconds
        {
            get { return Id > 0 ? Id : 60; }
        }

        private static readonly SynchronizedDictionary<string, CachedObject<KeyPair>> _cache =
            new SynchronizedDictionary<string, CachedObject<KeyPair>>(StringComparer.Ordinal);

        #endregion

        #region 方法

        Task<string> IOneOffKeyPairGrain.GetPublicKey(string name)
        {
            KeyPair keyPair = RSACryptoTextProvider.CreateKeyPair();
            _cache[name] = new CachedObject<KeyPair>(keyPair, DateTime.Now.AddSeconds(DiscardIntervalSeconds));
            return Task.FromResult(keyPair.PublicKey);
        }

        Task<string> IOneOffKeyPairGrain.Decrypt(string name, string cipherText, bool fOAEP)
        {
            string result = null;
            if (_cache.TryGetValue(name, out CachedObject<KeyPair> cachedObject))
            {
                result = RSACryptoTextProvider.Decrypt(cachedObject.Value.PrivateKey, cipherText, fOAEP);
                _cache.Remove(name);
            }

            return Task.FromResult(result);
        }

        #endregion
    }
}

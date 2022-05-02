namespace Phenix.Core.Security.Cryptography
{
    /// <summary>
    /// 公钥私钥对
    /// </summary>
    public sealed class KeyPair
    {
        internal KeyPair(string publicKey, string privateKey)
        {
            _publicKey = publicKey;
            _privateKey = privateKey;
        }

        #region 属性

        private readonly string _publicKey;

        /// <summary>
        /// 公钥
        /// </summary>
        public string PublicKey
        {
            get { return _publicKey; }
        }

        private readonly string _privateKey;

        /// <summary>
        /// 私钥
        /// </summary>
        public string PrivateKey
        {
            get { return _privateKey; }
        }

        #endregion
    }
}

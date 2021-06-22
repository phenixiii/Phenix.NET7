using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core;
using Phenix.Services.Contract.Security.Cryptography;

namespace Phenix.Services.Plugin.Security.Cryptography
{
    /// <summary>
    /// 一次性公钥私钥对控制器
    /// </summary>
    [Route(ApiConfig.ApiSecurityOneOffKeyPairPath)]
    [ApiController]
    public sealed class OneOffKeyPairController : Phenix.Core.Net.Api.ControllerBase
    {
        #region 属性

        #region 配置项

        private static int? _keyPairDiscardIntervalSeconds;

        /// <summary>
        /// 公钥私钥对丢弃间隔(秒)
        /// 默认：60
        /// </summary>
        public static int KeyPairDiscardIntervalSeconds
        {
            get { return AppSettings.GetProperty(ref _keyPairDiscardIntervalSeconds, 60); }
            set { AppSettings.SetProperty(ref _keyPairDiscardIntervalSeconds, value); }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 获取一次性公钥
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>公钥</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<string> GetPublicKey(string name)
        {
            return await ClusterClient.Default.GetGrain<IOneOffKeyPairGrain>(KeyPairDiscardIntervalSeconds).GetPublicKey(name);
        }
        
        #endregion
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Services.Plugin.Actor.Security;
using Phenix.Services.Plugin.Actor.Security.Cryptography;

namespace Phenix.Services.Plugin.Api.Security.Cryptography
{
    /// <summary>
    /// 一次性公钥私钥对控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityOneOffKeyPairPath)]
    [ApiController]
    public sealed class OneOffKeyPairController : Phenix.Core.Net.Api.ControllerBase
    {
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
            return await ClusterClient.Default.GetGrain<IOneOffKeyPairGrain>(UserGrain.KeyPairDiscardIntervalSeconds).GetPublicKey(name);
        }
        
        #endregion
    }
}

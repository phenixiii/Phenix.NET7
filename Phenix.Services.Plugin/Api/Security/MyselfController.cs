using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Data.Schema;

namespace Phenix.Services.Plugin.Api.Security
{
    /// <summary>
    /// 用户自己控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityMyselfPath)]
    [ApiController]
    public sealed class MyselfController : Phenix.Core.Net.Api.ControllerBase
    {
        // phAjax.getMyself()
        /// <summary>
        /// 获取自己资料
        /// </summary>
        /// <returns>自己资料</returns>
        [Authorize]
        [HttpGet]
        public async Task<string> Get()
        {
            return await EncryptAsync(await User.Identity.UserProxy.FetchKernel());
        }

        /// <summary>
        /// 更新自己资料
        /// </summary>
        /// <returns>更新记录数</returns>
        [Authorize]
        [HttpPatch]
        public async Task<int> Patch()
        {
            return await User.Identity.UserProxy.PatchKernel(await Request.ReadBodyAsNameValuesAsync(true));
        }
    }
}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Phenix.Services.Plugin.Api.Security.Myself
{
    /// <summary>
    /// 用户密码控制器
    /// </summary>
    [EnableCors]
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityMyselfPasswordPath)]
    [ApiController]
    public sealed class PasswordController : Phenix.Core.Net.Api.ControllerBase
    {
        // phAjax.changePassword()
        /// <summary>
        /// 修改登录口令
        /// </summary>
        [Authorize]
        [HttpPut]
        public async Task<bool> Put()
        {
            return await User.Identity.UserProxy.ChangePassword(await Request.ReadBodyAsStringAsync(true), true);
        }
    }
}
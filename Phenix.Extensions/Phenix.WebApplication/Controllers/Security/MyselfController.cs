using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Security;

namespace Phenix.WebApplication.Controllers.Security
{
    /// <summary>
    /// 用户自己控制器
    /// </summary>
    [EnableCors]
    [Route("/api/security/Myself")]
    [ApiController]
    public sealed class MyselfController : Phenix.Core.Net.ControllerBase
    {
        #region 方法

        // GET: /api/security/myself
        // phAjax.getMyself()
        /// <summary>
        /// 获取自己资料
        /// </summary>
        /// <returns>自己资料</returns>
        [Authorize]
        [HttpGet]
        public User Get()
        {
            return User.Identity.User;
        }

        // PATCH: /api/security/myself
        // phAjax.changePassword()
        /// <summary>
        /// 修改登录口令
        /// </summary>
        [Authorize]
        [HttpPatch]
        public bool ChangePassword()
        {
            return User.Identity.User.ChangePassword(DecryptBody());
        }

        #endregion
    }
}

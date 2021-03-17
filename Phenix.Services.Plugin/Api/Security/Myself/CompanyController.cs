using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Net.Filters;
using Phenix.Core.Security;
using Phenix.Services.Plugin.Actor.Security;

namespace Phenix.Services.Plugin.Api.Security.Myself
{
    /// <summary>
    /// 公司资料控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityMyselfCompanyPath)]
    [ApiController]
    public sealed class CompanyController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取公司资料
        /// </summary>
        /// <returns>公司资料</returns>
        [Authorize]
        [HttpGet]
        public async Task<Teams> Get()
        {
            return await ClusterClient.Default.GetGrain<ITeamsGrain>(User.Identity.CompanyName).FetchKernel();
        }

        /// <summary>
        /// 更新公司资料
        /// </summary>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task Patch()
        {
            await ClusterClient.Default.GetGrain<ITeamsGrain>(User.Identity.CompanyName).PatchKernel(await Request.ReadBodyAsync<Teams>());
        }
    }
}
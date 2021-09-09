using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Net.Filters;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract;
using Phenix.Services.Contract.Security.Myself;

namespace Phenix.Services.Plugin.Security.Myself
{
    /// <summary>
    /// 公司资料控制器
    /// </summary>
    [Route(WebApiConfig.SecurityMyselfCompanyPath)]
    [ApiController]
    public sealed class CompanyController : Phenix.Core.Net.Api.ControllerBase
    {
        // phAjax.getMyselfCompany()
        /// <summary>
        /// 获取公司资料
        /// </summary>
        /// <returns>公司资料</returns>
        [Authorize]
        [HttpGet]
        public async Task<Teams> Get()
        {
            return await ClusterClient.Default.GetGrain<ICompanyTeamsGrain>(User.Identity.CompanyName).FetchKernel();
        }

        /// <summary>
        /// 更新公司资料
        /// </summary>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task Patch()
        {
            await ClusterClient.Default.GetGrain<ICompanyTeamsGrain>(User.Identity.CompanyName).PatchKernel(await Request.ReadBodyAsync<Teams>());
        }
    }
}
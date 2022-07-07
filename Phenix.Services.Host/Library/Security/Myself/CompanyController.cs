using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Net.Filters;

namespace Phenix.Services.Host.Library.Security.Myself
{
    /// <summary>
    /// 公司资料控制器
    /// </summary>
    [Route(StandardPaths.SecurityMyselfCompanyPath)]
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
            return await ClusterClient.Default.GetGrain<ICompanyGrain>(User.Identity.CompanyName).FetchKernel();
        }

        /// <summary>
        /// 更新公司资料
        /// </summary>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task Patch()
        {
            await ClusterClient.Default.GetGrain<ICompanyGrain>(User.Identity.CompanyName).PatchKernel(await Request.ReadBodyAsync<Dictionary<string, object>>());
        }
    }
}
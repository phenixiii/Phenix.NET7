using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Actor.CustomerSecurity;
using Demo.IDOS.Plugin.Business.CustomerSecurity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Net.Filters;

namespace Demo.IDOS.Plugin.Api.CustomerSecurity
{
    /// <summary>
    /// 客户用户
    /// </summary>
    [Route(ApiConfig.ApiCustomerSecurityCustomerUserPath)]
    [ApiController]
    public sealed class CustomerUserController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取用户所属的客户
        /// </summary>
        /// <returns>客户用户</returns>
        [Authorize]
        [HttpGet]
        public IList<DcsCustomerUser> Get()
        {
            return DcsCustomerUser.FetchAll(Database.Default, p => p.UsId == User.Identity.Id && p.Disabled == false);
        }

        /// <summary>
        /// 某用户是否属于某客户
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>是否可用</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpGet]
        public async Task<bool> Usable(long customerId, long userId)
        {
            return await ClusterClient.Default.GetGrain<ICustomerUserGrain>(customerId, userId.ToString()).Usable();
        }

        /// <summary>
        /// 某用户属于某客户
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="userId">用户ID</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPut]
        public async Task Able(long customerId, long userId)
        {
            await ClusterClient.Default.GetGrain<ICustomerUserGrain>(customerId, userId.ToString()).Able();
        }

        /// <summary>
        /// 某用户不属于某客户
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="userId">用户ID</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpDelete]
        public async Task Disable(long customerId, long userId)
        {
            await ClusterClient.Default.GetGrain<ICustomerUserGrain>(customerId, userId.ToString()).Disable();
        }
    }
}

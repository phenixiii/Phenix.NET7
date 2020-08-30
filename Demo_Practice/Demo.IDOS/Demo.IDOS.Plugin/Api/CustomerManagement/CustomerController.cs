using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Actor.CustomerManagement;
using Demo.IDOS.Plugin.Business.CustomerManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data;
using Phenix.Core.Net.Filters;

namespace Demo.IDOS.Plugin.Api.CustomerManagement
{
    /// <summary>
    /// 客户
    /// </summary>
    [Route(ApiConfig.ApiCustomerManagementCustomerPath)]
    [ApiController]
    public sealed class CustomerController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取客户资料
        /// </summary>
        /// <returns>客户资料</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpGet]
        public IList<DcmCustomer> Get()
        {
            return DcmCustomer.FetchAll(Database.Default);
        }

        /// <summary>
        /// 获取客户资料
        /// </summary>
        /// <param name="id">客户ID</param>
        /// <returns>客户资料</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpGet]
        public async Task<DcmCustomer> Get(long id)
        {
            return await ClusterClient.Default.GetGrain<ICustomerGrain>(id).FetchKernel();
        }

        /// <summary>
        /// 更新客户资料(如不存在则新增)
        /// </summary>
        /// <param name="customer">客户</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task Patch([FromBody] DcmCustomer customer)
        {
            await ClusterClient.Default.GetGrain<ICustomerGrain>(customer.Id).PatchKernel(customer);
        }
    }
}

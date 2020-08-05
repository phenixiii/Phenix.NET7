using System;
using System.Security;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Actor.CustomerManagement;
using Demo.IDOS.Plugin.Actor.CustomerSecurity;
using Demo.IDOS.Plugin.Actor.DepotNorm;
using Demo.IDOS.Plugin.Actor.DepotSecurity;
using Phenix.Actor;
using Phenix.Core.Security;

namespace Demo.IDOS.Plugin.Filters
{
    /// <summary>
    /// 访问授权过滤器
    /// </summary>
    public class AuthorizationFilters
    {
        /// <summary>
        /// 检查仓库用户有效性
        /// </summary>
        /// <param name="identity">用户身份</param>
        /// <param name="depotId">仓库ID</param>
        public static async Task CheckDepotUserValidity(Identity identity, long depotId)
        {
            if (!await ClusterClient.Default.GetGrain<IDepotUserGrain>(depotId, identity.Id.ToString()).Usable())
                throw new SecurityException(String.Format("不允许 {0} 用户操作 {1} 仓库的数据!",
                    identity.Name,
                    (await ClusterClient.Default.GetGrain<IDepotGrain>(depotId).FetchKernel()).Name));
        }

        /// <summary>
        /// 检查客户用户有效性
        /// </summary>
        /// <param name="identity">用户身份</param>
        /// <param name="customerId">客户ID</param>
        public static async Task CheckCustomerUserValidity(Identity identity, long customerId)
        {
            if (!await ClusterClient.Default.GetGrain<ICustomerUserGrain>(customerId, identity.Id.ToString()).Usable())
                throw new SecurityException(String.Format("不允许 {0} 用户操作 {1} 客户的数据!",
                    identity.Name,
                    (await ClusterClient.Default.GetGrain<ICustomerGrain>(customerId).FetchKernel()).Name));
        }
    }
}

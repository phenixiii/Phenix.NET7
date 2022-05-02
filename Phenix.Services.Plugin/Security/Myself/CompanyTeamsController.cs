using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Net.Filters;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Security.Myself;

namespace Phenix.Services.Plugin.Security.Myself
{
    /// <summary>
    /// 公司团体控制器
    /// </summary>
    [Route(Phenix.Net.Api.Standards.SecurityMyselfCompanyTeamsPath)]
    [ApiController]
    public sealed class CompanyTeamsController : Phenix.Net.Api.ControllerBase
    {
        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="parentId">父节点ID</param>
        /// <returns>子节点ID</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPost]
        public async Task<long> AddChild(string name, long parentId)
        {
            return await ClusterClient.Default.GetGrain<ICompanyGrain>(User.Identity.CompanyName).AddChildNode(parentId, Teams.Set(p => p.Name, name));
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPut]
        public async Task ChangeParent(long id, long parentId)
        {
            await ClusterClient.Default.GetGrain<ICompanyGrain>(User.Identity.CompanyName).ChangeParentNode(id, parentId);
        }

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="name">名称</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task Update(long id, string name)
        {
            await ClusterClient.Default.GetGrain<ICompanyGrain>(User.Identity.CompanyName).UpdateNode(id, Teams.Set(p => p.Name, name));
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="id">节点ID</param>
        [CompanyAdminFilter]
        [Authorize]
        [HttpDelete]
        public async Task<int> Delete(long id)
        {
            return await ClusterClient.Default.GetGrain<ICompanyGrain>(User.Identity.CompanyName).DeleteBranch(id);
        }
    }
}
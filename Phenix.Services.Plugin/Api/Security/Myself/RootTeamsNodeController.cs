using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data.Schema;
using Phenix.Core.Net.Filters;
using Phenix.Core.Security;
using Phenix.Services.Plugin.Actor;

namespace Phenix.Services.Plugin.Api.Security.Myself
{
    /// <summary>
    /// 用户团体控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityMyselfRootTeamsNodePath)]
    [ApiController]
    public sealed class RootTeamsNodeController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="name">名称</param>
        /// <returns>子节点ID</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPut]
        public async Task<long> AddChild(string name, long parentId)
        {
            return User.Identity.RootTeamsProxy != null
                ? await User.Identity.RootTeamsProxy.AddChildNode(parentId, NameValue.Set<Teams>(p => p.Name, name))
                : await ClusterClient.Default.GetGrain<IUserGrain>(User.Identity.Name).PatchRootTeams(name);
        }

        /// <summary>
        /// 更改父节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="parentId">父节点ID</param>
        /// <returns>更新记录数</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPatch]
        public async Task<int> ChangeParent(long id, long parentId)
        {
            return User.Identity.RootTeamsProxy != null 
                ? await User.Identity.RootTeamsProxy.ChangeParentNode(id, parentId) 
                : -1;
        }

        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="name">名称</param>
        /// <returns>更新记录数</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpPost]
        public async Task<int> Update(long id, string name)
        {
            return User.Identity.RootTeamsProxy != null
                ? await User.Identity.RootTeamsProxy.UpdateNode(id, NameValue.Set<Teams>(p => p.Name, name))
                : -1;
        }

        /// <summary>
        /// 删除节点枝杈
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>更新记录数</returns>
        [CompanyAdminFilter]
        [Authorize]
        [HttpDelete]
        public async Task<int> Delete(long id)
        {
            return User.Identity.RootTeamsProxy != null
                ? await User.Identity.RootTeamsProxy.DeleteNode(id)
                : -1;
        }
    }
}
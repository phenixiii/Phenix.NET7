using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Data;
using Phenix.Core.Net.Filters;

namespace Phenix.Services.Plugin.Api.Security
{
    /// <summary>
    /// 用户角色控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiSecurityRolePath)]
    [ApiController]
    public sealed class RoleController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取角色
        /// </summary>
        /// <returns>角色</returns>
        [Authorize]
        [HttpGet]
        public IList<string> Get()
        {
            List<string> result = new List<string>();
            foreach (ControllerRole controllerRole in ControllerRole.FetchAll(Database.Default))
            foreach (string role in controllerRole.Roles)
            foreach (string s in role.Split(new char[] {'|', ','}, StringSplitOptions.RemoveEmptyEntries))
                if (!result.Contains(s))
                    result.Add(s);

            return result;
        }
    }
}
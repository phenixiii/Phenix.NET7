using System;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core.Security;

namespace Phenix.Services.Plugin.Actor
{
    /// <summary>
    /// 岗位资料Grain
    /// </summary>
    public class PositionGrain : EntityGrainBase<Position>, IPositionGrain
    {
        #region 方法

        Task<bool> IPositionGrain.IsInRole(string role)
        {
            if (String.IsNullOrEmpty(role))
                return Task.FromResult(true);
            bool foundRole = false;
            foreach (string s in role.Split('|', StringSplitOptions.RemoveEmptyEntries))
                if (!String.IsNullOrEmpty(s))
                {
                    if (Kernel.Roles != null && Kernel.Roles.Contains(s))
                        return Task.FromResult(true);
                    foundRole = true;
                }

            return Task.FromResult(!foundRole);
        }

        #endregion
    }
}
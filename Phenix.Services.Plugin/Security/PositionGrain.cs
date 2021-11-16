using System;
using System.Linq;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Plugin.Security
{
    /// <summary>
    /// 岗位资料Grain
    /// key: ID
    /// </summary>
    public class PositionGrain : EntityGrainBase<Position>, IPositionGrain
    {
        #region 方法

        Task<bool> IPositionGrain.IsInRole(string[] roles)
        {
            if (Kernel == null)
                throw new PositionNotFoundException();

            if (roles == null || roles.Length == 0)
                return Task.FromResult(true);

            foreach (string s in roles)
                if (String.IsNullOrEmpty(s) ||
                    Kernel.Roles != null && Kernel.Roles.Any(item => String.Compare(item, s, StringComparison.Ordinal) == 0))
                    return Task.FromResult(true);
            return Task.FromResult(false);
        }

        #endregion
    }
}
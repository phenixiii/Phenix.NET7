using System;
using System.Linq;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Actor.Security;

namespace Phenix.Services.Host.Library.Security
{
    /// <summary>
    /// 岗位资料Grain
    /// key: PositionId
    /// </summary>
    public class PositionGrain : EntityGrainBase<Position>, IPositionGrain
    {
        #region 属性

        /// <summary>
        /// ID
        /// </summary>
        protected long PositionId
        {
            get { return PrimaryKeyLong; }
        }

        #endregion

        #region 方法

        Task<bool> IPositionGrain.IsInRole(string[] roles)
        {
            if (Kernel == null)
                throw new InvalidOperationException(String.Format("岗位资料不存在!: {0}", PositionId));

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
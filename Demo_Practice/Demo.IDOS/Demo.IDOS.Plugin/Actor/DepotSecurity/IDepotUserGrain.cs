using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.DepotSecurity;
using Orleans;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.DepotSecurity
{
    /// <summary>
    /// 仓库用户
    /// </summary>
    public interface IDepotUserGrain : IEntityGrain<DdsDepotUser>, IGrainWithIntegerCompoundKey
    {
        /// <summary>
        /// 是否可用
        /// </summary>
        Task<bool> Usable();

        /// <summary>
        /// 可用
        /// </summary>
        Task Able();

        /// <summary>
        /// 禁用
        /// </summary>
        Task Disable();
    }
}

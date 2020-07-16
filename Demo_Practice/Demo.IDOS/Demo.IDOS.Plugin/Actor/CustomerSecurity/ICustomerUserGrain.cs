using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.CustomerSecurity;
using Orleans;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.CustomerSecurity
{
    /// <summary>
    /// 客户用户
    /// </summary>
    public interface ICustomerUserGrain : IEntityGrain<DcsCustomerUser>, IGrainWithIntegerCompoundKey
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

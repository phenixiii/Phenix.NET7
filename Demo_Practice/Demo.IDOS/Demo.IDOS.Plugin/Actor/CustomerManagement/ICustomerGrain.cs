using Demo.IDOS.Plugin.Business.CustomerManagement;
using Orleans;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.CustomerManagement
{
    /// <summary>
    /// 客户
    /// </summary>
    public interface ICustomerGrain : IEntityGrain<DcmCustomer>, IGrainWithIntegerKey
    {
    }
}

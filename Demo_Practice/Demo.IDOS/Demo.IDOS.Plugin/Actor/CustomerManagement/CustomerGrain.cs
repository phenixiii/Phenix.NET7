using Demo.IDOS.Plugin.Business.CustomerManagement;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.CustomerManagement
{
    /// <summary>
    /// 客户
    /// </summary>
    public class CustomerGrain : EntityGrainBase<DcmCustomer>, ICustomerGrain
    {
    }
}
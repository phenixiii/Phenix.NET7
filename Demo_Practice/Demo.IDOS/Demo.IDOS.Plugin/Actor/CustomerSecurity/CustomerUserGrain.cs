using System;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.CustomerSecurity;
using Phenix.Actor;
using Phenix.Core.Data.Model;
using Phenix.Core.Data.Schema;

namespace Demo.IDOS.Plugin.Actor.CustomerSecurity
{
    /// <summary>
    /// 客户用户
    /// </summary>
    public class CustomerUserGrain : EntityGrainBase<DcsCustomerUser>, ICustomerUserGrain
    {
        #region 属性

        private DcsCustomerUser _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override DcsCustomerUser Kernel
        {
            get { return _kernel ?? (_kernel = EntityBase<DcsCustomerUser>.FetchRoot(Database, 
                             p => p.CmId == Id && p.UsId == IdExtension.Value,
                             () => DcsCustomerUser.New(Database,
                                 NameValue.Set<DcsCustomerUser>(p => p.CmId, Id),
                                 NameValue.Set<DcsCustomerUser>(p => p.UsId, IdExtension)))); }
            set { _kernel = value; }
        }

        #endregion

        #region 方法
        
        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        /// <returns>更新记录数</returns>
        protected override int PatchKernel(params NameValue[] propertyValues)
        {
            throw new NotImplementedException("请用ICustomerUserGrain提供的接口间接操作Kernel对象");
        }

        Task<bool> ICustomerUserGrain.Usable()
        {
            return Task.FromResult(!Kernel.Disabled);
        }

        Task ICustomerUserGrain.Able()
        {
            Kernel.UpdateSelf(
                NameValue.Set<DcsCustomerUser>(p => p.Disabled, false));
            return Task.CompletedTask;
        }

        Task ICustomerUserGrain.Disable()
        {
            Kernel.UpdateSelf(
                NameValue.Set<DcsCustomerUser>(p => p.Disabled, true));
            return Task.CompletedTask;
        }

        #endregion

    }
}
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.CustomerSecurity;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.CustomerSecurity
{
    /// <summary>
    /// 客户用户
    /// </summary>
    public class CustomerUserGrain : EntityGrainBase<DcsCustomerUser>, ICustomerUserGrain
    {
        #region 属性
        
        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override DcsCustomerUser Kernel
        {
            get { return base.Kernel ?? (base.Kernel = DcsCustomerUser.FetchRoot(Database, 
                             p => p.CmId == Id && p.UsId == IdExtension,
                             () => DcsCustomerUser.New(Database,
                                 DcsCustomerUser.Set(p => p.CmId, Id).
                                     Set(p => p.UsId, IdExtension)))); }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        protected override void PatchKernel(DcsCustomerUser source)
        {
            throw new NotImplementedException("请用ICustomerUserGrain提供的接口间接操作Kernel对象");
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected override void PatchKernel(IDictionary<string, object> propertyValues)
        {
            throw new NotImplementedException("请用ICustomerUserGrain提供的接口间接操作Kernel对象");
        }

        Task<bool> ICustomerUserGrain.Usable()
        {
            return Task.FromResult(!Kernel.Disabled);
        }

        Task ICustomerUserGrain.Able()
        {
            Kernel.UpdateSelf(Kernel.SetProperty(p => p.Disabled, false));
            return Task.CompletedTask;
        }

        Task ICustomerUserGrain.Disable()
        {
            Kernel.UpdateSelf(Kernel.SetProperty(p => p.Disabled, true));
            return Task.CompletedTask;
        }

        #endregion

    }
}
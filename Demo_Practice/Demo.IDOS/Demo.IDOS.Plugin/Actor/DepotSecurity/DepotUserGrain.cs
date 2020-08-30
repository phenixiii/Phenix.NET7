using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.DepotSecurity;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.DepotSecurity
{
    /// <summary>
    /// 仓库用户
    /// </summary>
    public class DepotUserGrain : EntityGrainBase<DdsDepotUser>, IDepotUserGrain
    {
        #region 属性
        
        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override DdsDepotUser Kernel
        {
            get { return base.Kernel ?? (base.Kernel = DdsDepotUser.FetchRoot(Database, 
                             p => p.DpId == Id && p.UsId == IdExtension,
                             () => DdsDepotUser.New(Database,
                                 DdsDepotUser.Set(p => p.DpId, Id).
                                     Set(p => p.UsId, IdExtension)))); }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="source">数据源</param>
        protected override void PatchKernel(DdsDepotUser source)
        {
            throw new NotImplementedException("请用IDepotUserGrain提供的接口间接操作Kernel对象");
        }

        /// <summary>
        /// 更新根实体对象(如不存在则新增)
        /// </summary>
        /// <param name="propertyValues">待更新属性值队列</param>
        protected override void PatchKernel(IDictionary<string, object> propertyValues)
        {
            throw new NotImplementedException("请用IDepotUserGrain提供的接口间接操作Kernel对象");
        }

        Task<bool> IDepotUserGrain.Usable()
        {
            return Task.FromResult(!Kernel.Disabled);
        }

        Task IDepotUserGrain.Able()
        {
            Kernel.UpdateSelf(Kernel.SetProperty(p => p.Disabled, false));
            return Task.CompletedTask;
        }

        Task IDepotUserGrain.Disable()
        {
            Kernel.UpdateSelf(Kernel.SetProperty(p => p.Disabled, true));
            return Task.CompletedTask;
        }

        #endregion

    }
}
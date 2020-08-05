using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.DepotSecurity;
using Phenix.Actor;
using Phenix.Core.Data.Schema;

namespace Demo.IDOS.Plugin.Actor.DepotSecurity
{
    /// <summary>
    /// 仓库用户
    /// </summary>
    public class DepotUserGrain : EntityGrainBase<DdsDepotUser>, IDepotUserGrain
    {
        #region 属性

        private DdsDepotUser _kernel;

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override DdsDepotUser Kernel
        {
            get { return _kernel ?? (_kernel = DdsDepotUser.FetchRoot(Database, 
                             p => p.DpId == Id && p.UsId == IdExtension.Value,
                             () => DdsDepotUser.New(Database,
                                 NameValue.Set<DdsDepotUser>(p => p.DpId, Id),
                                 NameValue.Set<DdsDepotUser>(p => p.UsId, IdExtension)))); }
            set { _kernel = value; }
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
            Kernel.UpdateSelf(
                NameValue.Set<DdsDepotUser>(p => p.Disabled, false));
            return Task.CompletedTask;
        }

        Task IDepotUserGrain.Disable()
        {
            Kernel.UpdateSelf(
                NameValue.Set<DdsDepotUser>(p => p.Disabled, true));
            return Task.CompletedTask;
        }

        #endregion

    }
}
﻿using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Actor.Security;

namespace Phenix.Services.Host.Library.Security.Myself
{
    /// <summary>
    /// 公司Grain
    /// key: CompanyName
    /// </summary>
    public class CompanyGrain : TreeEntityGrainBase<Teams>, ICompanyGrain
    {
        #region 属性

        /// <summary>
        /// 公司名
        /// </summary>
        protected string CompanyName
        {
            get { return PrimaryKeyString; }
        }

        /// <summary>
        /// 根实体对象
        /// </summary>
        protected override Teams Kernel
        {
            get { return base.Kernel ??= Teams.FetchTree(Database, p => p.Name == CompanyName && p.Id == p.RootId); }
        }

        #endregion

        #region 方法

        Task<bool> ICompanyGrain.IsValid()
        {
            return Task.FromResult(Kernel != null && Kernel.Children.Length > 0);
        }

        #endregion
    }
}
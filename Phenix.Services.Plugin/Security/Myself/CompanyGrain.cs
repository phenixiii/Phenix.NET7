using Phenix.Actor;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Security.Myself;

namespace Phenix.Services.Plugin.Security.Myself
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
    }
}
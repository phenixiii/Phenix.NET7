using Phenix.Actor;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Security.Myself;

namespace Phenix.Services.Plugin.Security.Myself
{
    /// <summary>
    /// 公司团体Grain
    /// key: CompanyName
    /// </summary>
    public class CompanyTeamsGrain : TreeEntityGrainBase<Teams>, ICompanyTeamsGrain
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
            get
            {
                return base.Kernel ?? (base.Kernel = Teams.FetchTree(Database, p => p.Name == CompanyName && p.Id == p.RootId,
                    () => Teams.NewRoot(Database, Teams.Set(p => p.Name, CompanyName))));
            }
        }

        #endregion
    }
}
using Phenix.Actor;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Plugin.Security
{
    /// <summary>
    /// 团队资料Grain
    /// </summary>
    public class TeamsGrain : TreeEntityGrainBase<Teams>, ITeamsGrain
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
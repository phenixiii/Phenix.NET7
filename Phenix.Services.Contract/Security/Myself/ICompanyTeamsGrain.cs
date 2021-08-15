using Orleans;
using Phenix.Actor;
using Phenix.Services.Business.Security;

namespace Phenix.Services.Contract.Security.Myself
{
    /// <summary>
    /// 公司团队Grain接口
    /// key: CompanyName
    /// </summary>
    public interface ICompanyTeamsGrain : ITreeEntityGrain<Teams>, IGrainWithStringKey
    {
    }
}
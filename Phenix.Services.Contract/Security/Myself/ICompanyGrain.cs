using Orleans;
using Phenix.Actor;
using Phenix.Services.Business.Security;

namespace Phenix.Services.Contract.Security.Myself
{
    /// <summary>
    /// 公司Grain接口
    /// key: CompanyName
    /// </summary>
    public interface ICompanyGrain : ITreeEntityGrain<Teams>, IGrainWithStringKey
    {
    }
}
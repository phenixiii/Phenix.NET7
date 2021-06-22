using Orleans;
using Phenix.Actor;
using Phenix.Services.Business.Security;

namespace Phenix.Services.Contract.Security
{
    /// <summary>
    /// 团队资料Grain接口
    /// </summary>
    public interface ITeamsGrain : ITreeEntityGrain<Teams>, IGrainWithStringKey
    {
    }
}
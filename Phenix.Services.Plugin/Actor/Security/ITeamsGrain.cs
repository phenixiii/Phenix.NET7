using Orleans;
using Phenix.Actor;
using Phenix.Core.Security;

namespace Phenix.Services.Plugin.Actor.Security
{
    /// <summary>
    /// 团队资料Grain接口
    /// </summary>
    public interface ITeamsGrain : ITreeEntityGrain<Teams>, IGrainWithIntegerKey
    {
    }
}
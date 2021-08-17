using Orleans;

namespace Phenix.TPT.Contract
{
    /// <summary>
    /// 工作人员Grain接口
    /// key: RootTeamsId
    /// keyExtension: Worker
    /// </summary>
    public interface IWorkerGrain : Phenix.Actor.IGrain, IGrainWithIntegerCompoundKey
    {
    }
}

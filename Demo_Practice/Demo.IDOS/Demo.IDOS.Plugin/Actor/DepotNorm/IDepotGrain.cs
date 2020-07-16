using Demo.IDOS.Plugin.Business.DepotNorm;
using Orleans;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.DepotNorm
{
    /// <summary>
    /// 仓库
    /// </summary>
    public interface IDepotGrain : IEntityGrain<DdnDepot>, IGrainWithIntegerKey
    {
    }
}

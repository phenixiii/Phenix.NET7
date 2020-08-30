using Demo.IDOS.Plugin.Business.DepotNorm;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.DepotNorm
{
    /// <summary>
    /// 仓库
    /// </summary>
    public class DepotGrain : EntityGrainBase<DdnDepot>, IDepotGrain
    {
    }
}
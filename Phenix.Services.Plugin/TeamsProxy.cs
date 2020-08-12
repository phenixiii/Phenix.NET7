using Phenix.Actor;
using Phenix.Core.Security;
using Phenix.Services.Plugin.Actor.Security;

namespace Phenix.Services.Plugin
{
    /// <summary>
    /// 团队资料代理
    /// </summary>
    public sealed class TeamsProxy : TreeEntityGrainProxyBase<TeamsProxy, Teams, ITeamsGrain>, ITeamsProxy
    {
    }
}

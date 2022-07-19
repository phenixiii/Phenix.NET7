using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Phenix.Actor.Security;
using Phenix.Core.Security;

namespace Phenix.Actor.Filters
{
    /// <summary>
    /// 入站消息拦截器
    /// </summary>
    public class IncomingGrainCallFilter : IIncomingGrainCallFilter
    {
        async Task IIncomingGrainCallFilter.Invoke(IIncomingGrainCallContext context)
        {
            Principal.CurrentIdentity =
                RequestContext.Get(ContextKeys.CurrentIdentityCompanyName) is string companyName &&
                RequestContext.Get(ContextKeys.CurrentIdentityUserName) is string userName &&
                RequestContext.Get(ContextKeys.CurrentIdentityCultureName) is string cultureName
                    ? Identity.Fetch(companyName, userName, cultureName, null)
                    : null;
            await context.Invoke();
        }
    }
}
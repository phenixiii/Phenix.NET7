using System;
using System.Globalization;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Log;
using Phenix.Core.Security;

namespace Phenix.Actor.Filters
{
    internal class OutgoingGrainCallFilter : IOutgoingGrainCallFilter
    {
        async Task IOutgoingGrainCallFilter.Invoke(IOutgoingGrainCallContext context)
        {
            IIdentity currentIdentity = Principal.CurrentIdentity;
            if (currentIdentity != null)
            {
                RequestContext.Set(ContextKeys.CurrentIdentityCompanyName, currentIdentity.CompanyName);
                RequestContext.Set(ContextKeys.CurrentIdentityUserName, currentIdentity.UserName);
                RequestContext.Set(ContextKeys.CurrentIdentityCultureName, currentIdentity.CultureName);
            }

            if (RequestContext.Get(ContextKeys.TraceKey) == null)
                RequestContext.Set(ContextKeys.TraceKey, Database.Default.Sequence.Value);

            object traceOrder = RequestContext.Get(ContextKeys.TraceOrder);
            RequestContext.Set(ContextKeys.TraceOrder, traceOrder != null ? (int)traceOrder + 1 : 0);

            DateTime dateTime = DateTime.Now;
            try
            {
                await context.Invoke();

                if (AppRun.Debugging)
                    LogHelper.Debug("{@Context} consume time {@TotalMilliseconds} ms",
                        new
                        {
                            TraceKey = RequestContext.Get(ContextKeys.TraceKey),
                            TraceOrder = RequestContext.Get(ContextKeys.TraceOrder),
                            Type = context.Grain.GetType().FullName,
                            Method = context.InterfaceMethod.Name,
                            Arguments = context.Arguments,
                        },
                        DateTime.Now.Subtract(dateTime).TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "{@Context} consume time {@TotalMilliseconds} ms",
                    new
                    {
                        TraceKey = RequestContext.Get(ContextKeys.TraceKey),
                        TraceOrder = RequestContext.Get(ContextKeys.TraceOrder),
                        Type = context.Grain.GetType().FullName,
                        Method = context.InterfaceMethod.Name,
                        Arguments = context.Arguments,
                    },
                    DateTime.Now.Subtract(dateTime).TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                throw;
            }
        }
    }
}
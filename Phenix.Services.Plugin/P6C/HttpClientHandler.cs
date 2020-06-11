using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core.Net;
using Phenix.Core.Security;

namespace Phenix.Services.Plugin.P6C
{
    internal class HttpClientHandler : System.Net.Http.HttpClientHandler
    {
        #region 方法

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Put || request.Method == HttpMethod.Patch || request.Method == HttpMethod.Delete)
            {
                request.Headers.Add(NetConfig.MethodOverrideHeaderName, request.Method.ToString());
                request.Method = HttpMethod.Post;
            }

            Identity identity = Identity.CurrentIdentity;
            if (identity != null)
            {
                string value = Guid.NewGuid().ToString();
                value = String.Format("{0},{1},{2}", Uri.EscapeDataString(identity.Name), value, identity.UserProxy.Encrypt(value));
                request.Headers.Add(NetConfig.AuthorizationHeaderName, value);
            }

            return base.SendAsync(request, cancellationToken);
        }

        #endregion
    }
}

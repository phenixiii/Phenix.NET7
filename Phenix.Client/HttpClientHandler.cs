using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core.Net;
using Phenix.Core.Net.Api;

namespace Phenix.Client
{
    internal class HttpClientHandler : System.Net.Http.HttpClientHandler
    {
        #region 属性

        private HttpClient _owner;

        public HttpClient Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        #endregion

        #region 方法

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Put || request.Method == HttpMethod.Patch || request.Method == HttpMethod.Delete)
            {
                request.Headers.Add(NetConfig.MethodOverrideHeaderName, request.Method.ToString());
                request.Method = HttpMethod.Post;
            }

            if (Owner.Identity != null)
                request.Headers.Add(NetConfig.AuthorizationHeaderName, Owner.Identity.User.FormatComplexAuthorization(
                    String.Compare(request.RequestUri.AbsolutePath, ApiConfig.ApiSecurityGatePath, StringComparison.OrdinalIgnoreCase) == 0 && request.Method == HttpMethod.Post));

            return base.SendAsync(request, cancellationToken);
        }

        #endregion
    }
}
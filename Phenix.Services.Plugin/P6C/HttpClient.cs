using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Phenix.Core;
using Phenix.Core.Data.Schema;
using Phenix.Core.Net;
using Phenix.Core.Reflection;
using Phenix.Core.Security;
using Phenix.Core.Security.Auth;

namespace Phenix.Services.Plugin.P6C
{
    /// <summary>
    /// HttpClient
    /// 对接Phenix.NET6提供的WabAPI服务
    /// </summary>
    public class HttpClient : System.Net.Http.HttpClient
    {
        private HttpClient(HttpClientHandler handler)
            : base(handler)
        {
        }

        #region 单例

        private static readonly object _defaultLock = new object();
        private static HttpClient _default;

        /// <summary>
        /// 单例
        /// </summary>
        public static HttpClient Default
        {
            get
            {
                if (!ExistThirdParty)
                    return null;

                if (_default == null)
                    lock (_defaultLock)
                        if (_default == null)
                        {
                            _default = new HttpClient(new HttpClientHandler());
                            _default.BaseAddress = new Uri(ThirdPartyService);
                        }

                return _default;
            }
        }

        #endregion

        #region 属性

        #region 配置项

        private static bool? _existThirdParty;

        /// <summary>
        /// 存在第三方服务
        /// 默认：false
        /// </summary>
        public static bool ExistThirdParty
        {
            get { return AppSettings.GetProperty(ref _existThirdParty, false); }
            set { AppSettings.SetProperty(ref _existThirdParty, value); }
        }

        private static string _thirdPartyService;

        /// <summary>
        /// 第三方服务地址
        /// 默认：http://localhost:8080
        /// </summary>
        public static string ThirdPartyService
        {
            get { return AppSettings.GetProperty(ref _thirdPartyService, "http://localhost:8080"); }
            set { AppSettings.SetProperty(ref _thirdPartyService, value); }
        }

        #endregion

        private const string SecurityUri = "/api/Security";

        #endregion

        #region 方法

        #region Security

        /// <summary>
        /// 登录核实
        /// </summary>
        /// <param name="userName">登录名</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="signature">签名</param>
        /// <param name="tag">捎带数据(未解密, 默认是客户端当前时间)</param>
        /// <returns>提示信息</returns>
        public async Task<string> LogonAsync(string userName, string timestamp, string signature, string tag)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, SecurityUri))
            {
                request.Headers.Add(NetConfig.AuthorizationHeaderName, String.Format("{0},{1},{2}", Uri.EscapeDataString(userName), timestamp, signature));
                request.Content = new StringContent(tag, Encoding.UTF8);
                using (HttpResponseMessage response = await SendAsync(request))
                {
                    await response.ThrowIfFailedAsync();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        #endregion

        #region Call

        private string BuildPathAndQuery(string path, NameValue[] queryValues)
        {
            if (queryValues != null && queryValues.Length > 0)
            {
                StringBuilder queryBuilder = new StringBuilder();
                foreach (NameValue item in queryValues)
                    queryBuilder.AppendFormat("{0}={1}&", item.Name, item.Value);
                queryBuilder.Remove(queryBuilder.Length - 1, 1);
                return String.Format("{0}?{1}", path, queryBuilder.ToString());
            }

            return path;
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="queryValues">查询条件值</param>
        /// <returns>返回对象</returns>
        public async Task<T> CallAsync<T>(HttpMethod method, string path, params NameValue[] queryValues)
        {
            return await CallAsync<T>(method, path, null, false, false, queryValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="decryptResult">是否解密返回数据（服务端的控制器代码请用Encrypt(result)加密）</param>
        /// <param name="queryValues">查询条件值</param>
        /// <returns>返回对象</returns>
        public async Task<T> CallAsync<T>(HttpMethod method, string path, bool decryptResult, params NameValue[] queryValues)
        {
            return await CallAsync<T>(method, path, null, false, decryptResult, queryValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="data">上传数据</param>
        /// <param name="queryValues">查询条件值</param>
        /// <returns>返回对象</returns>
        public async Task<T> CallAsync<T>(HttpMethod method, string path, object data, params NameValue[] queryValues)
        {
            return await CallAsync<T>(method, path, data, false, false, queryValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="data">上传数据</param>
        /// <param name="encryptData">是否加密上传数据（服务端的控制器代码请用Request.ReadBodyXXX(true)解密）</param>
        /// <param name="queryValues">查询条件值</param>
        /// <returns>返回对象</returns>
        public async Task<T> CallAsync<T>(HttpMethod method, string path, object data, bool encryptData, params NameValue[] queryValues)
        {
            return await CallAsync<T>(method, path, data, encryptData, false, queryValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="data">上传数据</param>
        /// <param name="encryptData">是否加密上传数据（服务端的控制器代码请用Request.ReadBodyXXX(true)解密）(如果data是HttpContent则忽略)</param>
        /// <param name="decryptResult">是否解密返回数据（服务端的控制器代码请用Encrypt(result)加密）</param>
        /// <param name="queryValues">查询条件值</param>
        /// <returns>返回对象</returns>
        public async Task<T> CallAsync<T>(HttpMethod method, string path, object data, bool encryptData, bool decryptResult, params NameValue[] queryValues)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            Identity identity = Identity.CurrentIdentity;
            if (identity == null)
                throw new AuthenticationException();
            if (!identity.IsAuthenticated)
                throw new UserVerifyException();

            using (HttpRequestMessage request = new HttpRequestMessage(method, BuildPathAndQuery(path, queryValues)))
            {
                if (data != null)
                    if (data is HttpContent httpContent)
                        request.Content = httpContent;
                    else
                    {
                        if (data is NameValue nameValue)
                            data = NameValue.ToArray(nameValue);
                        request.Content = new StringContent(encryptData ? await identity.UserProxy.Encrypt(Utilities.JsonSerialize(data)) : Utilities.JsonSerialize(data), Encoding.UTF8);
                    }

                using (HttpResponseMessage response = await SendAsync(request))
                {
                    await response.ThrowIfFailedAsync();
                    return Utilities.JsonDeserialize<T>(decryptResult ? await identity.UserProxy.Decrypt(await response.Content.ReadAsStringAsync()) : await response.Content.ReadAsStringAsync());
                }
            }
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="queryValues">查询条件值</param>
        public async Task CallAsync(HttpMethod method, string path, params NameValue[] queryValues)
        {
            await CallAsync(method, path, null, false, queryValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="data">上传数据</param>
        /// <param name="queryValues">查询条件值</param>
        public async Task CallAsync(HttpMethod method, string path, object data, params NameValue[] queryValues)
        {
            await CallAsync(method, path, data, false, queryValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="data">上传数据</param>
        /// <param name="encryptData">是否加密上传数据（服务端的控制器代码请用Request.ReadBodyXXX(true)解密）(如果data是HttpContent则忽略)</param>
        /// <param name="queryValues">查询条件值</param>
        public async Task CallAsync(HttpMethod method, string path, object data, bool encryptData, params NameValue[] queryValues)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            Identity identity = Identity.CurrentIdentity;
            if (identity == null)
                throw new AuthenticationException();
            if (!identity.IsAuthenticated)
                throw new UserVerifyException();

            using (HttpRequestMessage request = new HttpRequestMessage(method, BuildPathAndQuery(path, queryValues)))
            {
                if (data != null)
                    if (data is HttpContent httpContent)
                        request.Content = httpContent;
                    else
                    {
                        if (data is NameValue nameValue)
                            data = NameValue.ToArray(nameValue);
                        request.Content = new StringContent(encryptData ? await identity.UserProxy.Encrypt(Utilities.JsonSerialize(data)) : Utilities.JsonSerialize(data), Encoding.UTF8);
                    }

                using (HttpResponseMessage response = await SendAsync(request))
                {
                    await response.ThrowIfFailedAsync();
                }
            }
        }

        #endregion

        #endregion
    }
}
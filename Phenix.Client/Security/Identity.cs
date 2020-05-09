using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Phenix.Client.Security
{
    /// <summary>
    /// 用户身份
    /// </summary>
    public sealed class Identity : IIdentity
    {
        internal Identity(HttpClient httpClient, string name, string password)
        {
            _httpClient = httpClient;
            _user = new User(this, name, password);
        }

        #region 工厂

        /// <summary>
        /// 当前用户身份
        /// </summary>
        public static Identity CurrentIdentity
        {
            get { return HttpClient.Default != null ? HttpClient.Default.Identity : Thread.CurrentPrincipal != null ? Thread.CurrentPrincipal.Identity as Identity : null; }
            set { Thread.CurrentPrincipal = value != null ? new Principal(value) : null; }
        }

        #endregion

        #region 属性

        private readonly HttpClient _httpClient;

        /// <summary>
        /// HttpClient
        /// </summary>
        public HttpClient HttpClient
        {
            get { return _httpClient; }
        }

        private User _user;

        /// <summary>
        /// 用户资料
        /// </summary>
        public User User
        {
            get { return _user; }
        }

        /// <summary>
        /// 登录名
        /// </summary>
        public string Name
        {
            get { return _user.Name; }
        }

        /// <summary>
        /// 已身份验证?
        /// </summary>
        public bool IsAuthenticated
        {
            get { return _user.IsAuthenticated; }
        }

        /// <summary>
        /// 身份验证类型
        /// </summary>
        public string AuthenticationType
        {
            get { return "Phenix-Authorization"; }
        }

        #endregion

        #region 方法

        internal async Task LogonAsync(string tag)
        {
            await _user.LogonAsync(tag);
            _user = await ReFetchUserAsync();
        }

        /// <summary>
        /// 重新获取用户资料
        /// </summary>
        public async Task<User> ReFetchUserAsync()
        {
            _user = await _user.ReFetchAsync();
            return _user;
        }

        #endregion
    }
}
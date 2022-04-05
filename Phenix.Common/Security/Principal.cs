using System;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Common.Threading;

namespace Phenix.Common.Security
{
    /// <summary>
    /// 用户
    /// </summary>
    public sealed class Principal : IPrincipal
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="identity">用户身份</param>
        public Principal(IIdentity identity)
        {
            _identity = identity;
        }

        #region 工厂

        /// <summary>
        /// 当前用户
        /// </summary>
        public static Principal CurrentPrincipal
        {
            get
            {
                if (Thread.CurrentPrincipal is Principal result)
                    return result;
                IIdentity identity = CurrentIdentity;
                return identity != null ? new Principal(identity) : null;
            }
            set { Thread.CurrentPrincipal = value; }
        }

        /// <summary>
        /// 当前用户身份
        /// </summary>
        public static IIdentity CurrentIdentity
        {
            get { return Thread.CurrentPrincipal != null ? Thread.CurrentPrincipal.Identity as IIdentity : null; }
            set
            {
                if (value != null)
                {
                    Thread.CurrentPrincipal = new Principal(value);
                    if (!String.IsNullOrEmpty(value.CultureName))
                        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(value.CultureName);
                }
                else
                    Thread.CurrentPrincipal = null;
            }
        }

        private static Func<string, string, string, int?, IIdentity> _fetchIdentity;

        /// <summary>
        /// 获取用户身份方法
        /// Identity Fetch(string companyName, string userName, string cultureName, int? cacheDiscardIntervalHours = null)
        /// </summary>
        public static Func<string, string, string, int?, IIdentity> FetchIdentity
        {
            get { return _fetchIdentity; }
            set { _fetchIdentity = value; }
        }

        #endregion

        #region 属性
        
        #region 配置项

        private static int? _requestFailureCountMaximum;

        /// <summary>
        /// 服务请求失败次数极限
        /// 默认：5(>=3)
        /// </summary>
        public static int RequestFailureCountMaximum
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _requestFailureCountMaximum, 5), 3}.Max(); }
            set { AppSettings.SetLocalProperty(ref _requestFailureCountMaximum, new[] {value, 3}.Max()); }
        }

        private static int? _requestFailureLockedMinutes;

        /// <summary>
        /// 服务请求失败锁定周期(分钟)
        /// 默认：30(>=10)
        /// </summary>
        public static int RequestFailureLockedMinutes
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _requestFailureLockedMinutes, 30), 10}.Max(); }
            set { AppSettings.SetLocalProperty(ref _requestFailureLockedMinutes, new[] {value, 10}.Max()); }
        }

        private static bool? _allowMultiAddressRequest;

        /// <summary>
        /// 允许多处终端发起请求
        /// 默认：false
        /// </summary>
        public static bool AllowMultiAddressRequest
        {
            get { return AppRun.Debugging || AppSettings.GetLocalProperty(ref _allowMultiAddressRequest, false); }
            set { AppSettings.SetLocalProperty(ref _allowMultiAddressRequest, value); }
        }

        private static int? _requestIdleIntervalLimitMinutes;

        /// <summary>
        /// 服务请求空闲间隔极限(分钟)
        /// 默认：120(>=10)
        /// </summary>
        public static int RequestIdleIntervalLimitMinutes
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _requestIdleIntervalLimitMinutes, 120), 10}.Max(); }
            set { AppSettings.SetLocalProperty(ref _requestIdleIntervalLimitMinutes, new[] {value, 10}.Max()); }
        }

        private static int? _requestClockOffsetLimitMinutes;

        /// <summary>
        /// 服务请求(客户端与服务端)时钟差极限(分钟)
        /// 默认：30(>=10)
        /// </summary>
        public static int RequestClockOffsetLimitMinutes
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _requestClockOffsetLimitMinutes, 30), 10}.Max(); }
            set { AppSettings.SetLocalProperty(ref _requestClockOffsetLimitMinutes, new[] {value, 10}.Max()); }
        }

        private static int? _breakRequestIntensityPerMinute;

        /// <summary>
        /// 中断服务请求强度阈值(每分钟次数)
        /// 默认：6000(>=6000)
        /// </summary>
        public static int BreakRequestIntensityPerMinute
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _breakRequestIntensityPerMinute, 6000), 6000}.Max(); }
            set { AppSettings.SetLocalProperty(ref _breakRequestIntensityPerMinute, new[] {value, 6000}.Max()); }
        }

        private static int? _passwordLengthMinimum;

        /// <summary>
        /// 口令长度最小值
        /// 默认：6(>=6)
        /// </summary>
        public static int PasswordLengthMinimum
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _passwordLengthMinimum, 6), 6}.Max(); }
            set { AppSettings.SetLocalProperty(ref _passwordLengthMinimum, new[] {value, 6}.Max()); }
        }

        private static int? _passwordComplexityMinimum;

        /// <summary>
        /// 口令复杂度最小值(含数字、大写字母、小写字母、特殊字符的种类)
        /// 默认：3(>=1)
        /// </summary>
        public static int PasswordComplexityMinimum
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _passwordComplexityMinimum, 3), 1}.Max(); }
            set { AppSettings.SetLocalProperty(ref _passwordComplexityMinimum, new[] {value, 1}.Max()); }
        }

        private static int? _dynamicPasswordValidityMinutes;

        /// <summary>
        /// 动态口令有效周期(分钟)
        /// 默认：10(>=3)
        /// </summary>
        public static int DynamicPasswordValidityMinutes
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _dynamicPasswordValidityMinutes, 10), 3}.Max(); }
            set { AppSettings.SetLocalProperty(ref _dynamicPasswordValidityMinutes, new[] {value, 3}.Max()); }
        }

        #endregion

        private readonly IIdentity _identity;

        /// <summary>
        /// 用户身份
        /// </summary>
        public IIdentity Identity
        {
            get { return _identity; }
        }

        System.Security.Principal.IIdentity IPrincipal.Identity
        {
            get { return Identity; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 确定是否属于指定的角色
        /// </summary>
        /// <param name="role">角色</param>
        /// <returns>属于指定的角色</returns>
        public async Task<bool> IsInRoleAsync(string role)
        {
            IIdentity identity = Identity;
            if (identity == null)
                return false;
            if (!identity.IsAuthenticated)
                return false;
            if (!String.IsNullOrEmpty(role))
                foreach (string s in role.Split(','))
                    if (!await identity.IsInRole(s.Split('|')))
                        return false;
            return true;
        }

        #region IPrincipal 成员

        /// <summary>
        /// 确定是否属于指定的角色
        /// </summary>
        /// <param name="role">角色</param>
        /// <returns>属于指定的角色</returns>
        public bool IsInRole(string role)
        {
            return AsyncHelper.RunSync(() => IsInRoleAsync(role));
        }

        #endregion

        #endregion
    }
}
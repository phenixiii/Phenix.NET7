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
    /// �û�
    /// </summary>
    public sealed class Principal : IPrincipal
    {
        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="identity">�û����</param>
        public Principal(IIdentity identity)
        {
            _identity = identity;
        }

        #region ����

        /// <summary>
        /// ��ǰ�û�
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
        /// ��ǰ�û����
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
        /// ��ȡ�û���ݷ���
        /// Identity Fetch(string companyName, string userName, string cultureName, int? cacheDiscardIntervalHours = null)
        /// </summary>
        public static Func<string, string, string, int?, IIdentity> FetchIdentity
        {
            get { return _fetchIdentity; }
            set { _fetchIdentity = value; }
        }

        #endregion

        #region ����
        
        #region ������

        private static int? _requestFailureCountMaximum;

        /// <summary>
        /// ��������ʧ�ܴ�������
        /// Ĭ�ϣ�5(>=3)
        /// </summary>
        public static int RequestFailureCountMaximum
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _requestFailureCountMaximum, 5), 3}.Max(); }
            set { AppSettings.SetLocalProperty(ref _requestFailureCountMaximum, new[] {value, 3}.Max()); }
        }

        private static int? _requestFailureLockedMinutes;

        /// <summary>
        /// ��������ʧ����������(����)
        /// Ĭ�ϣ�30(>=10)
        /// </summary>
        public static int RequestFailureLockedMinutes
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _requestFailureLockedMinutes, 30), 10}.Max(); }
            set { AppSettings.SetLocalProperty(ref _requestFailureLockedMinutes, new[] {value, 10}.Max()); }
        }

        private static bool? _allowMultiAddressRequest;

        /// <summary>
        /// ����ദ�ն˷�������
        /// Ĭ�ϣ�false
        /// </summary>
        public static bool AllowMultiAddressRequest
        {
            get { return AppRun.Debugging || AppSettings.GetLocalProperty(ref _allowMultiAddressRequest, false); }
            set { AppSettings.SetLocalProperty(ref _allowMultiAddressRequest, value); }
        }

        private static int? _requestIdleIntervalLimitMinutes;

        /// <summary>
        /// ����������м������(����)
        /// Ĭ�ϣ�120(>=10)
        /// </summary>
        public static int RequestIdleIntervalLimitMinutes
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _requestIdleIntervalLimitMinutes, 120), 10}.Max(); }
            set { AppSettings.SetLocalProperty(ref _requestIdleIntervalLimitMinutes, new[] {value, 10}.Max()); }
        }

        private static int? _requestClockOffsetLimitMinutes;

        /// <summary>
        /// ��������(�ͻ���������)ʱ�Ӳ��(����)
        /// Ĭ�ϣ�30(>=10)
        /// </summary>
        public static int RequestClockOffsetLimitMinutes
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _requestClockOffsetLimitMinutes, 30), 10}.Max(); }
            set { AppSettings.SetLocalProperty(ref _requestClockOffsetLimitMinutes, new[] {value, 10}.Max()); }
        }

        private static int? _breakRequestIntensityPerMinute;

        /// <summary>
        /// �жϷ�������ǿ����ֵ(ÿ���Ӵ���)
        /// Ĭ�ϣ�6000(>=6000)
        /// </summary>
        public static int BreakRequestIntensityPerMinute
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _breakRequestIntensityPerMinute, 6000), 6000}.Max(); }
            set { AppSettings.SetLocalProperty(ref _breakRequestIntensityPerMinute, new[] {value, 6000}.Max()); }
        }

        private static int? _passwordLengthMinimum;

        /// <summary>
        /// �������Сֵ
        /// Ĭ�ϣ�6(>=6)
        /// </summary>
        public static int PasswordLengthMinimum
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _passwordLengthMinimum, 6), 6}.Max(); }
            set { AppSettings.SetLocalProperty(ref _passwordLengthMinimum, new[] {value, 6}.Max()); }
        }

        private static int? _passwordComplexityMinimum;

        /// <summary>
        /// ����Ӷ���Сֵ(�����֡���д��ĸ��Сд��ĸ�������ַ�������)
        /// Ĭ�ϣ�3(>=1)
        /// </summary>
        public static int PasswordComplexityMinimum
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _passwordComplexityMinimum, 3), 1}.Max(); }
            set { AppSettings.SetLocalProperty(ref _passwordComplexityMinimum, new[] {value, 1}.Max()); }
        }

        private static int? _dynamicPasswordValidityMinutes;

        /// <summary>
        /// ��̬������Ч����(����)
        /// Ĭ�ϣ�10(>=3)
        /// </summary>
        public static int DynamicPasswordValidityMinutes
        {
            get { return new[] {AppSettings.GetLocalProperty(ref _dynamicPasswordValidityMinutes, 10), 3}.Max(); }
            set { AppSettings.SetLocalProperty(ref _dynamicPasswordValidityMinutes, new[] {value, 3}.Max()); }
        }

        #endregion

        private readonly IIdentity _identity;

        /// <summary>
        /// �û����
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

        #region ����

        /// <summary>
        /// ȷ���Ƿ�����ָ���Ľ�ɫ
        /// </summary>
        /// <param name="role">��ɫ</param>
        /// <returns>����ָ���Ľ�ɫ</returns>
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

        #region IPrincipal ��Ա

        /// <summary>
        /// ȷ���Ƿ�����ָ���Ľ�ɫ
        /// </summary>
        /// <param name="role">��ɫ</param>
        /// <returns>����ָ���Ľ�ɫ</returns>
        public bool IsInRole(string role)
        {
            return AsyncHelper.RunSync(() => IsInRoleAsync(role));
        }

        #endregion

        #endregion
    }
}
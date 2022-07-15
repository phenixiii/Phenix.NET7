using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core.Threading;

namespace Phenix.Core.Security
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

        #endregion

        #region ����

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
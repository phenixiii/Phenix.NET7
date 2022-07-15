using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Phenix.Core.Threading;

namespace Phenix.Core.Security
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

        #endregion

        #region 属性

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
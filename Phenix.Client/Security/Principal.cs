using System;
using System.Security.Principal;
using System.Threading;

namespace Phenix.Client.Security
{
    /// <summary>
    /// 用户
    /// </summary>
    public sealed class Principal : IPrincipal
    {
        internal Principal(Identity identity)
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
                Identity identity = Identity.CurrentIdentity;
                return identity != null ? new Principal(identity) : null;
            }
            set { Thread.CurrentPrincipal = value; }
        }

        #endregion

        #region 属性

        private readonly Identity _identity;

        /// <summary>
        /// 用户身份
        /// </summary>
        public Identity Identity
        {
            get { return _identity; }
        }

        /// <summary>
        /// 用户身份
        /// </summary>
        IIdentity IPrincipal.Identity
        {
            get { return Identity; }
        }

        #endregion

        #region 方法

        #region IPrincipal 成员

        /// <summary>
        /// 确定是否属于指定的角色
        /// </summary>
        /// <param name="role">角色</param>
        /// <returns>属于指定的角色</returns>
        public bool IsInRole(string role)
        {
            Identity identity = Identity;
            if (identity == null)
                return false;
            if (!identity.IsAuthenticated)
                return false;
            if (String.IsNullOrEmpty(role))
                return true;
            return identity.User.Position != null && identity.User.Position.IsInRole(role);
        }

        #endregion

        #endregion
    }
}
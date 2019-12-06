using System.Security.Principal;
using System.Threading;

namespace Phenix.Client.Security
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
        public Principal(Identity identity)
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
        public bool IsInRole(string role)
        {
            return Identity != null && Identity.IsInRole(role);
        }

        #endregion

        #endregion
    }
}
using System;
using System.Security.Principal;
using System.Threading;

namespace Phenix.Client.Security
{
    /// <summary>
    /// 用户身份
    /// </summary>
    public sealed class Identity : IIdentity
    {
        internal Identity(User user)
        {
            _user = user;
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

        /// <summary>
        /// Guest登录名
        /// </summary>
        public const string GuestUserName = "GUEST";

        /// <summary>
        /// 管理员登录名
        /// </summary>
        public const string AdminUserName = "ADMIN";

        private User _user;

        /// <summary>
        /// 用户资料
        /// </summary>
        public User User
        {
            get { return _user; }
            internal set { _user = value; }
        }

        /// <summary>
        /// 登录名
        /// </summary>
        public string Name
        {
            get { return _user.Name; }
        }

        private bool _isAuthenticated;

        /// <summary>
        /// 已身份验证?
        /// </summary>
        public bool IsAuthenticated
        {
            get { return _isAuthenticated && !_user.Disabled && !_user.Locked; }
            internal set { _isAuthenticated = value; }
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

        /// <summary>
        /// 确定是否属于指定的角色
        /// </summary>
        /// <param name="role">角色</param>
        public bool IsInRole(string role)
        {
            if (!IsAuthenticated)
                return false;
            return User.Position != null && User.Position.Roles.Contains(role) ||
                   String.CompareOrdinal(User.Name, AdminUserName) == 0;
        }

        #endregion
    }
}
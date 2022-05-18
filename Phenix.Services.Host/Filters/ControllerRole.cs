using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Phenix.Business;
using Phenix.Core.Data;
using Phenix.Core.Net.Filters;
using Phenix.Core.Security;
using Phenix.Core.Security.Auth;
using Phenix.Core.SyncCollections;

namespace Phenix.Services.Host.Filters
{
    /// <summary>
    /// 控制器的授权角色
    /// </summary>
    [Serializable]
    public class ControllerRole : EntityBase<ControllerRole>
    {
        /// <summary>
        /// for CreateInstance
        /// </summary>
        protected ControllerRole()
        {
            //禁止添加代码
        }

        /// <summary>
        /// for Newtonsoft.Json.JsonConstructor
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        protected ControllerRole(string dataSourceKey,
            long id, string controllerName, string actionName, string[] roles)
            : base(dataSourceKey)
        {
            _id = id;
            _controllerName = controllerName;
            _actionName = actionName;
            _roles = roles;
        }

        private ControllerRole(string dataSourceKey, long id, TypeInfo typeInfo, MethodInfo methodInfo)
            : this(dataSourceKey, id, typeInfo.FullName, methodInfo.Name, FetchRoles((methodInfo)) ?? FetchRoles(typeInfo))
        {
            _typeInfo = typeInfo;
            _methodInfo = methodInfo;
            IList<AuthorizationFilterAttribute> filter = FetchFilters(methodInfo) ?? FetchFilters(typeInfo);
            _filters = filter != null ? new ReadOnlyCollection<AuthorizationFilterAttribute>(filter) : null;
        }

        #region 工厂

        private static readonly SynchronizedDictionary<string, ControllerRole> _cache = new SynchronizedDictionary<string, ControllerRole>(StringComparer.Ordinal);

        /// <summary>
        /// 获取控制器的授权角色
        /// </summary>
        /// <param name="typeInfo">TypeInfo</param>
        /// <param name="methodInfo">MethodInfo</param>
        /// <returns>控制器的授权角色</returns>
        public static ControllerRole Fetch(TypeInfo typeInfo, MethodInfo methodInfo)
        {
            return _cache.GetValue(Standards.FormatCompoundKey(typeInfo.FullName, methodInfo.Name, methodInfo.GetHashCode()), () =>
                FetchRoot(Database.Default, p => p.ControllerName == typeInfo.FullName && p.ActionName == methodInfo.Name,
                    () => new ControllerRole(Database.Default.DataSourceKey, Database.Default.Sequence.Value, typeInfo, methodInfo))
            );
        }

        private static string[] FetchRoles(MemberInfo element)
        {
            List<string> result = null;
            foreach (Attribute item in Attribute.GetCustomAttributes(element))
                if (item is IAllowAnonymous)
                    return null;
                else if (item is IAuthorizeData authorizeData)
                {
                    if (result == null)
                        result = new List<string>();
                    result.Add(authorizeData.Roles);
                }

            return result != null ? result.ToArray() : null;
        }

        private static AuthorizationFilterAttribute[] FetchFilters(MemberInfo element)
        {
            Attribute[] result = Attribute.GetCustomAttributes(element, typeof(AuthorizationFilterAttribute));
            if (result.Length > 0)
                return result as AuthorizationFilterAttribute[];
            return null;
        }

        #endregion

        #region 属性

        [NonSerialized]
        private TypeInfo _typeInfo;

        /// <summary>
        /// 方法所在类
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public TypeInfo TypeInfo
        {
            get { return _typeInfo; }
        }

        [NonSerialized]
        private MethodInfo _methodInfo;

        /// <summary>
        /// 方法信息
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
        }

        private long _id;

        /// <summary>
        /// 主键
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        private string _controllerName;

        /// <summary>
        /// Controller全名
        /// </summary>
        public string ControllerName
        {
            get { return _controllerName; }
        }

        private string _actionName;

        /// <summary>
        /// Action名
        /// </summary>
        public string ActionName
        {
            get { return _actionName; }
        }

        private string[] _roles;

        /// <summary>
        /// 角色清单
        /// 多个角色用‘|’分隔，互相为 or 关系
        /// 多组角色个用‘,’分隔，互相为 and 关系
        /// </summary>
        public string[] Roles
        {
            get { return _roles; }
        }

        [NonSerialized]
        private readonly ReadOnlyCollection<AuthorizationFilterAttribute> _filters;

        /// <summary>
        /// 访问授权过滤器
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IList<AuthorizationFilterAttribute> Filters
        {
            get { return _filters; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 检查有效性
        /// </summary>
        /// <param name="identity">用户身份</param>
        /// <param name="context">HttpContext</param>
        public async Task CheckValidityAsync(IIdentity identity, HttpContext context)
        {
            if (_roles != null && _roles.Length > 0)
            {
                if (identity == null)
                    throw new AuthenticationException();
                if (!identity.IsAuthenticated)
                    throw new UserVerifyException();
                
                foreach (string s1 in _roles)
                foreach (string s2 in s1.Split(','))
                    if (!await identity.IsInRole(s2.Split('|')))
                        throw new SecurityException();
            }

            if (_filters != null && _filters.Count > 0)
            {
                if (identity == null)
                    throw new AuthenticationException();
                if (!identity.IsAuthenticated)
                    throw new UserVerifyException();

                foreach (AuthorizationFilterAttribute item in _filters)
                    await item.CheckValidity(identity, context);
            }
        }

        #endregion
    }
}
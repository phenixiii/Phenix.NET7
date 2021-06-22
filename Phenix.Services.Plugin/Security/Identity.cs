using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Phenix.Actor;
using Phenix.Core;
using Phenix.Core.Data;
using Phenix.Core.Reflection;
using Phenix.Core.Security;
using Phenix.Core.SyncCollections;
using Phenix.Core.Threading;
using Phenix.Services.Business.Security;
using Phenix.Services.Contract.Security;

namespace Phenix.Services.Plugin.Security
{
    /// <summary>
    /// �û�����
    /// </summary>
    public sealed class Identity : IIdentity
    {
        private Identity(string companyName, string userName, string cultureName)
        {
            _companyName = companyName;
            _userName = userName;
            _cultureName = cultureName;
        }

        #region ����
        
        #region ������

        private static int? _cacheDiscardIntervalHours;

        /// <summary>
        /// ���涪�����(Сʱ)
        /// Ĭ�ϣ�1
        /// </summary>
        public static int CacheDiscardIntervalHours
        {
            get { return AppSettings.GetProperty(ref _cacheDiscardIntervalHours, 1); }
            set { AppSettings.SetProperty(ref _cacheDiscardIntervalHours, value); }
        }

        #endregion

        private static readonly SynchronizedDictionary<string, CachedObject<Identity>> _cache =
            new SynchronizedDictionary<string, CachedObject<Identity>>(StringComparer.Ordinal);

        /// <summary>
        /// ��ȡ�û�����
        /// </summary>
        /// <param name="companyName">��˾��</param>
        /// <param name="userName">��¼��</param>
        /// <param name="cultureName">����������</param>
        /// <param name="cacheDiscardIntervalHours">���涪�����(Сʱ)</param>
        /// <returns>�û�����</returns>
        public static Identity Fetch(string companyName, string userName, string cultureName, int? cacheDiscardIntervalHours = null)
        {
            if (String.IsNullOrEmpty(companyName))
                return null;
            if (String.IsNullOrEmpty(userName))
                return null;

            string key = FormatPrimaryKey(companyName, userName);
            cacheDiscardIntervalHours = cacheDiscardIntervalHours ?? CacheDiscardIntervalHours;
            if (cacheDiscardIntervalHours > 0 && _cache.TryGetValue(key, out CachedObject<Identity> cachedObject))
            {
                cachedObject.Value._cultureName = cultureName;
                return cachedObject.Value;
            }

            Identity result = new Identity(companyName, userName, cultureName);
            _cache[key] = new CachedObject<Identity>(result, DateTime.Now.AddHours(cacheDiscardIntervalHours.Value));
            return result;
        }

        #endregion

        #region ����

        private string _primaryKey;

        /// <summary>
        /// PrimaryKey
        /// </summary>
        public string PrimaryKey
        {
            get { return _primaryKey ??= FormatPrimaryKey(_companyName, _userName); }
        }

        private readonly string _companyName;

        /// <summary>
        /// ��˾��
        /// </summary>
        public string CompanyName
        {
            get { return _companyName; }
        }

        private readonly string _userName;

        /// <summary>
        /// ��¼��
        /// </summary>
        public string UserName
        {
            get { return _userName; }
        }

        string System.Security.Principal.IIdentity.Name
        {
            get { return UserName; }
        }

        private string _cultureName;

        /// <summary>
        /// ����������
        /// </summary>
        public string CultureName
        {
            get { return _cultureName; }
        }

        private long? _id;

        /// <summary>
        /// ��������(ӳ���ID�ֶ�)
        /// </summary>
        public long Id
        {
            get { return _id ??= AsyncHelper.RunSync(() => GetKernelProperty(p => p.Id)); }
        }

        private long? _rootTeamsId;

        /// <summary>
        /// ������˾ID
        /// </summary>
        public long RootTeamsId
        {
            get { return _rootTeamsId ??= AsyncHelper.RunSync(() => GetKernelProperty(p => p.RootTeamsId)); }
        }

        private long? _teamsId;

        /// <summary>
        /// ��������ID
        /// </summary>
        public long TeamsId
        {
            get { return _teamsId ??= AsyncHelper.RunSync(() => GetKernelProperty(p => p.TeamsId)); }
        }

        private long? _positionId;

        /// <summary>
        /// ��λ����ID
        /// </summary>
        public long? PositionId
        {
            get { return _positionId ??= AsyncHelper.RunSync(() => GetKernelProperty(p => p.PositionId)); }
        }

        private bool? _isCompanyAdmin;
        /// <summary>
        /// �Ƿ�˾����Ա?
        /// </summary>
        public bool IsCompanyAdmin
        {
            get { return _isCompanyAdmin ??= AsyncHelper.RunSync(() => GetKernelProperty(p => p.IsCompanyAdmin)); }
        }

        /// <summary>
        /// ��������֤?
        /// </summary>
        public bool IsAuthenticated
        {
            get { return AsyncHelper.RunSync(() => GetKernelProperty(p => p.IsAuthenticated)); }
        }
        
        /// <summary>
        /// ������֤����
        /// </summary>
        public string AuthenticationType
        {
            get { return "Phenix-Authorization"; }
        }

        #endregion

        #region ����
        
        private static string FormatPrimaryKey(string companyName, string userName)
        {
            return String.Format("{0}{1}{2}", companyName, Standards.RowSeparator, userName);
        }

        /// <summary>
        /// ��ʽ��PrimaryKey
        /// </summary>
        string IIdentity.FormatPrimaryKey(string userName)
        {
            return FormatPrimaryKey(CompanyName, userName);
        }

        Task<bool> IIdentity.IsValidLogon(string timestamp, string signature, string tag, string requestAddress, string requestSession, bool throwIfNotConform)
        {
            return ClusterClient.Default.GetGrain<IUserGrain>(PrimaryKey).IsValidLogon(timestamp, signature, tag, requestAddress, requestSession, throwIfNotConform);
        }

        Task<bool> IIdentity.IsValid(string timestamp, string signature, string requestAddress, string requestSession, bool throwIfNotConform)
        {
            return ClusterClient.Default.GetGrain<IUserGrain>(PrimaryKey).IsValid(timestamp, signature, requestAddress, requestSession, throwIfNotConform);
        }

        Task<string> IIdentity.Encrypt(object sourceData)
        {
            return ClusterClient.Default.GetGrain<IUserGrain>(PrimaryKey).Encrypt(sourceData is string sourceText ? sourceText : Utilities.JsonSerialize(sourceData));
        }

        Task<string> IIdentity.Decrypt(string cipherText)
        {
            return ClusterClient.Default.GetGrain<IUserGrain>(PrimaryKey).Decrypt(cipherText);
        }

        Task<bool> IIdentity.IsInRole(string role)
        {
            return PositionId.HasValue ? ClusterClient.Default.GetGrain<IPositionGrain>(PositionId.Value).IsInRole(role) : Task.FromResult(false);
        }

        private Task<TValue> GetKernelProperty<TValue>(Expression<Func<User, TValue>> propertyLambda)
        {
            return ClusterClient.Default.GetGrain<IUserGrain>(PrimaryKey).GetKernelProperty<TValue>(Utilities.GetPropertyInfo<User>(propertyLambda).Name);
        }

        #endregion
    }
}
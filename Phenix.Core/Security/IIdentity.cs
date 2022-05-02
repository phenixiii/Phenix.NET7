using System.Threading.Tasks;

namespace Phenix.Core.Security
{
    /// <summary>
    /// 用户身份接口
    /// </summary>
    public interface IIdentity : System.Security.Principal.IIdentity
    {
        #region 属性

        /// <summary>
        /// CompanyName'\u0004'UserName
        /// </summary>
        string PrimaryKey { get; }

        /// <summary>
        /// 公司名
        /// </summary>
        string CompanyName { get; }

        /// <summary>
        /// 登录名
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// 区域性名称
        /// </summary>
        string CultureName { get; }

        /// <summary>
        /// 主键属性(映射表ID字段)
        /// </summary>
        long Id { get; }

        /// <summary>
        /// 所属公司ID
        /// </summary>
        long RootTeamsId { get; }

        /// <summary>
        /// 所属部门ID
        /// </summary>
        long TeamsId { get; }

        /// <summary>
        /// 岗位资料ID
        /// </summary>
        long? PositionId { get; }

        /// <summary>
        /// 是否公司管理员?
        /// </summary>
        bool IsCompanyAdmin { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 格式化PrimaryKey=CompanyName'\u0004'keyExtension
        /// </summary>
        /// <param name="keyExtension">扩展主键</param>
        string FormatPrimaryKey(string keyExtension);

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="signature">会话签名</param>
        /// <param name="tag">(默认是客户端时间也可以是修改的新密码)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        Task Logon(string signature, string tag, string requestAddress);

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="signature">会话签名</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        Task Verify(string signature, string requestAddress);

        /// <summary>
        /// 加密
        /// Key/IV=登录口令/动态口令
        /// </summary>
        /// <param name="sourceData">需加密的对象/字符串</param>
        /// <returns>密文(Base64字符串)</returns>
        Task<string> Encrypt(object sourceData);

        /// <summary>
        /// 解密
        ///  Key/IV=登录口令/动态口令
        /// </summary>
        /// <param name="cipherText">密文(Base64字符串)</param>
        Task<string> Decrypt(string cipherText);

        /// <summary>
        /// 确定是否属于指定的一组角色
        /// </summary>
        /// <param name="roles">指定的一组角色</param>
        /// <returns>存在交集</returns>
        Task<bool> IsInRole(params string[] roles);

        #endregion
    }
}
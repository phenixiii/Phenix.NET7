using System;

namespace Phenix.Core.Security
{
    /// <summary>
    /// 用户资料接口
    /// </summary>
    public interface IUser
    {
        #region 属性

        /// <summary>
        /// 主键
        /// </summary>
        long Id { get; }
        
        /// <summary>
        /// 登录名
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 手机
        /// </summary>
        string Phone { get; }

        /// <summary>
        /// 邮箱
        /// </summary>
        string EMail { get; }
        
        /// <summary>
        /// 注册昵称
        /// </summary>
        string RegAlias { get; }
        
        /// <summary>
        /// 注册时间
        /// </summary>
        DateTime RegTime { get; }

        /// <summary>
        /// 服务请求方IP地址
        /// </summary>
        string RequestAddress { get; }
        
        /// <summary>
        /// 服务请求会话签名
        /// </summary>
        string RequestSignature { get; }
        
        /// <summary>
        /// 服务请求失败次数
        /// </summary>
        int RequestFailureCount { get; }
        
        /// <summary>
        /// 服务请求失败时间
        /// </summary>
        DateTime? RequestFailureTime { get; }
        
        /// <summary>
        /// 所属公司ID
        /// </summary>
        long RootTeamsId { get; }

        /// <summary>
        /// 所属部门ID
        /// </summary>
        long TeamsId { get; }
        
        /// <summary>
        /// 担任岗位ID
        /// </summary>
        long? PositionId { get; }
        
        /// <summary>
        /// 是否锁定
        /// </summary>
        bool Locked { get; }

        /// <summary>
        /// 锁定时间
        /// </summary>
        DateTime? LockedTime { get; }

        /// <summary>
        /// 是否注销
        /// </summary>
        bool Disabled { get; }

        /// <summary>
        /// 注销时间
        /// </summary>
        DateTime? DisabledTime { get; }
        
        /// <summary>
        /// 登录口令(散列值)
        /// </summary>
        string Password { get; }

        /// <summary>
        /// 动态口令(散列值)
        /// 为空时用登录口令
        /// </summary>
        public string DynamicPassword { get; }

        /// <summary>
        /// 动态口令生成时间
        /// </summary>
        public DateTime? DynamicPasswordCreateTime { get; }

        /// <summary>
        /// 是否初始口令?
        /// </summary>
        bool IsInitialPassword { get; }

        /// <summary>
        /// 是否公司管理员?
        /// </summary>
        bool IsCompanyAdmin { get; }

        /// <summary>
        /// 已身份验证?
        /// </summary>
        bool IsAuthenticated { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 加密
        /// Key/IV=登录口令/动态口令
        /// </summary>
        /// <param name="data">需加密的对象/字符串</param>
        /// <returns>密文(Base64字符串)</returns>
        string Encrypt(object data);

        /// <summary>
        /// 解密
        ///  Key/IV=登录口令/动态口令
        /// </summary>
        /// <param name="cipherText">密文(Base64字符串)</param>
        /// <returns>原文</returns>
        string Decrypt(string cipherText);

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="signature">会话签名</param>
        /// <param name="tag">捎带数据(默认是客户端时间也可以是修改的新密码)</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        void Logon(string signature, ref string tag, string requestAddress);

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="signature">会话签名</param>
        /// <param name="requestAddress">服务请求方IP地址</param>
        void Verify(string signature, string requestAddress);

        /// <summary>
        /// 登出
        /// </summary>
        void Logout();

        /// <summary>
        /// 重置登录口令
        /// </summary>
        void ResetPassword();

        /// <summary>
        /// 修改登录口令
        /// </summary>
        /// <param name="newPassword">新登录口令</param>
        void ChangePassword(string newPassword);

        /// <summary>
        /// 申请动态口令
        /// </summary>
        /// <param name="requestAddress">服务请求方IP地址</param>
        /// <returns>动态口令(6位数字一般作为验证码用短信发送给到用户)</returns>
        string ApplyDynamicPassword(string requestAddress);

        /// <summary>
        /// 激活
        /// </summary>
        void Activate();

        /// <summary>
        /// 注销
        /// </summary>
        void Disable();
        
        #endregion
    }
}
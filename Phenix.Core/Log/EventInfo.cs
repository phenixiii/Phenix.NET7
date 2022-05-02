using System;
using System.Reflection;
using Phenix.Core.Data;
using Phenix.Core.Net;
using Phenix.Core.Security;

namespace Phenix.Core.Log
{
    /// <summary>
    /// 事件资料
    /// </summary>
    [Serializable]
    public class EventInfo
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="address">地址</param>
        /// <param name="error">错误</param>
        public EventInfo(long id, MethodBase method, string message, string address = null, Exception error = null)
            : this(id, DateTime.Now,
                method != null ? (method.ReflectedType ?? method.DeclaringType).FullName : null,
                method != null ? method.Name : null,
                message,
                error != null ? error.GetType().FullName : null,
                error != null ? AppRun.GetErrorMessage(error) : null,
                Principal.CurrentIdentity != null ? Principal.CurrentIdentity.UserName : null,
                address ?? NetConfig.LocalAddress, null, null)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="traceKey">调用链键值</param>
        /// <param name="traceOrder">调用链顺序</param>
        /// <param name="error">错误</param>
        public EventInfo(long id, MethodBase method, string message, long traceKey, int traceOrder, Exception error = null)
            : this(id, DateTime.Now,
                method != null ? (method.ReflectedType ?? method.DeclaringType).FullName : null,
                method != null ? method.Name : null,
                message,
                error != null ? error.GetType().FullName : null,
                error != null ? AppRun.GetErrorMessage(error) : null,
                Principal.CurrentIdentity != null ? Principal.CurrentIdentity.UserName : null,
                NetConfig.LocalAddress, traceKey, traceOrder)
        {
        }

        [Newtonsoft.Json.JsonConstructor]
        private EventInfo(long id, DateTime time,
            string className, string methodName, string message, string exceptionName, string exceptionMessage, string user, string address, long? traceKey, int? traceOrder)
        {
            _id = id != 0 ? id : Database.Default.Sequence.Value;
            _time = time > DateTime.MinValue ? time : DateTime.Now;
            _className = className;
            _methodName = methodName;
            _message = message;
            _exceptionName = exceptionName;
            _exceptionMessage = exceptionMessage;
            _user = user != null ? user : Principal.CurrentIdentity != null ? Principal.CurrentIdentity.UserName : null;
            _address = address;
            _traceKey = traceKey;
            _traceOrder = traceOrder;
        }

        #region 属性

        private readonly long _id;

        /// <summary>
        /// ID
        /// </summary>
        public long Id
        {
            get { return _id; }
        }

        private readonly DateTime _time;

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time
        {
            get { return _time; }
        }

        private readonly string _className;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName
        {
            get { return _className; }
        }

        private readonly string _methodName;

        /// <summary>
        /// 方法名
        /// </summary>
        public string MethodName
        {
            get { return _methodName; }
        }

        private readonly string _message;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message
        {
            get { return _message; }
        }

        private readonly string _exceptionName;

        /// <summary>
        /// 错误名/消息名
        /// </summary>
        public string ExceptionName
        {
            get { return _exceptionName; }
        }

        private readonly string _exceptionMessage;

        /// <summary>
        /// 错误消息/补充消息
        /// </summary>
        public string ExceptionMessage
        {
            get { return _exceptionMessage; }
        }

        private readonly string _user;

        /// <summary>
        /// 用户
        /// </summary>
        public string User
        {
            get { return _user; }
        }

        private readonly string _address;

        /// <summary>
        /// 地址
        /// </summary>
        public string Address
        {
            get { return _address; }
        }

        private readonly long? _traceKey;

        /// <summary>
        /// 调用链键值
        /// </summary>
        public long? traceKey
        {
            get { return _traceKey; }
        }

        private readonly int? _traceOrder;

        /// <summary>
        /// 调用链顺序
        /// </summary>
        public int? traceOrder
        {
            get { return _traceOrder; }
        }

        #endregion
    }
}
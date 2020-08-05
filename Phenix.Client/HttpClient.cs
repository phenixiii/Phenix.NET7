using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Phenix.Client.Security;
using Phenix.Core;
using Phenix.Core.Data.Schema;
using Phenix.Core.IO;
using Phenix.Core.Net;
using Phenix.Core.Net.Api;
using Phenix.Core.Reflection;
using Phenix.Core.Security.Auth;
using Phenix.Core.Security.Cryptography;

namespace Phenix.Client
{
    /// <summary>
    /// HttpClient
    /// </summary>
    public class HttpClient : System.Net.Http.HttpClient
    {
        private HttpClient(HttpClientHandler handler)
            : base(handler)
        {
            handler.Owner = this;
        }

        #region 工厂

        private static HttpClient _default;

        /// <summary>
        /// 缺省HttpClient
        /// 缺省为第一个调用LogonAsync成功的HttpClient对象
        /// </summary>
        public static HttpClient Default
        {
            get { return _default; }
            set { _default = value; }
        }

        /// <summary>
        /// 新增HttpClient
        /// </summary>
        /// <param name="baseAddress">服务地址(null代表"http://localhost:5000")</param>
        /// <returns>HttpClient</returns>
        public static HttpClient New(Uri baseAddress = null)
        {
            HttpClient result = new HttpClient(new HttpClientHandler());
            result.BaseAddress = baseAddress ?? new Uri("http://localhost:5000");
            return result;
        }

        #endregion

        #region 属性

        private Identity _identity;

        /// <summary>
        /// 登录身份(调用LogonAsync成功)
        /// </summary>
        public Identity Identity
        {
            get { return _identity; }
            private set
            {
                _identity = value;
                if (value != null && _default == null)
                    _default = this;
            }
        }

        #endregion

        #region 方法

        #region Security

        /// <summary>
        /// 登记/注册(获取动态口令)
        /// </summary>
        /// <param name="name">登录名</param>
        /// <param name="hashName">登录名需Hash</param>
        /// <param name="phone">手机</param>
        /// <param name="eMail">邮箱</param>
        /// <param name="regAlias">注册昵称</param>
        /// <returns>提示信息</returns>
        public async Task<string> CheckInAsync(string name, bool hashName = false, string phone = null, string eMail = null, string regAlias = null)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                String.Format("{0}?name={1}&phone={2}&eMail={3}&regAlias={4}", ApiConfig.ApiSecurityGatePath,
                    hashName && !Phenix.Core.Security.User.IsReservedUserName(name) ? MD5CryptoTextProvider.ComputeHash(name) : name, 
                    phone, eMail, regAlias)))
            {
                using (HttpResponseMessage response = await SendAsync(request))
                {
                    await response.ThrowIfFailedAsync();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="name">登录名</param>
        /// <param name="password">登录口令/动态口令(一般通过邮箱发送给到用户)</param>
        /// <param name="hashName">登录名需Hash</param>
        /// <param name="tag">捎带数据(默认是客户端当前时间)</param>
        /// <returns>用户身份</returns>
        public async Task<Identity> LogonAsync(string name, string password, bool hashName = false, string tag = null)
        {
            Identity = new Identity(this, hashName && !Phenix.Core.Security.User.IsReservedUserName(name) ? MD5CryptoTextProvider.ComputeHash(name) : name, password);
            await Identity.LogonAsync(tag);
            return Identity;
        }

        /// <summary>
        /// 获取一次性公钥
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>公钥</returns>
        public async Task<string> GetOneOffPublicKeyAsync(string name)
        {
            return await CallAsync<string>(HttpMethod.Get, ApiConfig.ApiSecurityOneOffKeyPairPath, 
                NameValue.Set(nameof(name), name));
        }

        #endregion

        #region Data

        /// <summary>
        /// 获取64位序号
        /// </summary>
        /// <returns>64位序号</returns>
        public async Task<long> GetSequenceAsync()
        {
            return await CallAsync<long>(HttpMethod.Get, ApiConfig.ApiDataSequencePath);
        }

        /// <summary>
        /// 获取64位增量
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="initialValue">初值</param>
        /// <returns>64位增量</returns>
        public async Task<long> GetIncrementAsync(string key, long initialValue = 1)
        {
            return await CallAsync<long>(HttpMethod.Get, ApiConfig.ApiDataIncrementPath,
                NameValue.Set(nameof(key), key),
                NameValue.Set(nameof(initialValue), initialValue));
        }

        #endregion

        #region Message

        /// <summary>
        /// 接收消息（PULL）
        /// </summary>
        /// <returns>结果集(消息ID-消息内容)</returns>
        public async Task<IDictionary<long, string>> ReceiveMessageAsync()
        {
            return await CallAsync<IDictionary<long, string>>(HttpMethod.Get, ApiConfig.ApiMessageUserMessagePath);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="receiver">接收用户</param>
        /// <param name="content">消息内容</param>
        public async Task SendMessageAsync(long id, string receiver, string content)
        {
            await CallAsync(HttpMethod.Put, ApiConfig.ApiMessageUserMessagePath, content,
                NameValue.Set(nameof(id), id),
                NameValue.Set(nameof(receiver), receiver));
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="receiver">接收用户</param>
        /// <param name="content">消息内容</param>
        public async Task SendMessageAsync(string receiver, string content)
        {
            await CallAsync(HttpMethod.Post, ApiConfig.ApiMessageUserMessagePath, content,
                NameValue.Set(nameof(receiver), receiver));
        }

        /// <summary>
        /// 确认收到
        /// </summary>
        /// <param name="id">消息ID</param>
        /// <param name="burn">是否销毁</param>
        public async Task AffirmReceivedMessageAsync(long id, bool burn = false)
        {
            await CallAsync(HttpMethod.Delete, ApiConfig.ApiMessageUserMessagePath,
                NameValue.Set(nameof(id), id),
                NameValue.Set(nameof(burn), burn));
        }

        /// <summary>
        /// 订阅消息（PUSH）
        /// </summary>
        /// <param name="onReceived">处理收到消息(消息ID-消息内容)</param>
        /// <returns>HubConnection可添加连接事件最后务必启动连接</returns>
        public HubConnection SubscribeMessage(Action<IDictionary<long, string>> onReceived)
        {
            if (Identity == null)
                throw new AuthenticationException();
            if (!Identity.IsAuthenticated)
                throw new UserVerifyException();

            HubConnection result = new HubConnectionBuilder()
                .WithUrl(BaseAddress.OriginalString + ApiConfig.ApiMessageUserMessageHubPath,
                    options => { options.AccessTokenProvider = () => Task.FromResult(Identity.User.FormatComplexAuthorization()); })
                .AddMessagePackProtocol()
                .WithAutomaticReconnect()
                .Build();

            result.On<string>("onReceived",
                message => { onReceived(Utilities.JsonDeserialize<IDictionary<long, string>>(message)); });
            return result;
        }

        #endregion

        #region InoutFile

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="targetPath">写入路径(文件名被上传)</param>
        /// <param name="doProgress">执行进度的回调函数, 回调函数返回值如为false则中止下载</param>
        public async Task DownloadFileAsync(string message, string targetPath, Func<FileChunkInfo, bool> doProgress)
        {
            if (String.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException(nameof(targetPath));

            try
            {
                using (FileStream fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                {
                    await DownloadFileAsync(message, Path.GetFileName(targetPath), fileStream, doProgress);
                }
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(targetPath))
                    File.Delete(targetPath);
                throw;
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="fileName">下载文件名</param>
        /// <param name="targetStream">写入文件流</param>
        /// <param name="doProgress">执行进度的回调函数, 回调函数返回值如为false则中止下载</param>
        public async Task DownloadFileAsync(string message, string fileName, Stream targetStream, Func<FileChunkInfo, bool> doProgress)
        {
            if (targetStream == null)
                throw new ArgumentNullException(nameof(targetStream));

            for (int i = 1; i < Int32.MaxValue; i++)
            {
                FileChunkInfo chunkInfo = await DownloadFileChunkAsync(message, fileName, i);
                if (doProgress != null && !doProgress(chunkInfo))
                    break;
                if (chunkInfo == null)
                    break;
                targetStream.Seek(chunkInfo.MaxChunkSize * (chunkInfo.ChunkNumber - 1), SeekOrigin.Begin);
                await targetStream.WriteAsync(chunkInfo.ChunkBody);
                await targetStream.FlushAsync();
                if (chunkInfo.Over)
                    break;
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="fileName">下载文件名</param>
        /// <param name="chunkNumber">下载文件块号</param>
        /// <returns>下载的文件块信息</returns>
        public async Task<FileChunkInfo> DownloadFileChunkAsync(string message, string fileName, int chunkNumber)
        {
            return await CallAsync<FileChunkInfo>(HttpMethod.Get, ApiConfig.ApiInoutFilePath,
                NameValue.Set(nameof(message), message),
                NameValue.Set(nameof(fileName), fileName),
                NameValue.Set(nameof(chunkNumber), chunkNumber));
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="sourcePath">读取路径(文件名被上传)</param>
        /// <param name="doProgress">执行进度的回调函数, 回调函数返回值如为false则中止上传</param>
        /// <returns>完成上传时返回消息</returns>
        public async Task<string> UploadFileAsync(string message, string sourcePath, Func<FileChunkInfo, bool> doProgress)
        {
            if (String.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));

            using (Stream fileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return await UploadFileAsync(message, Path.GetFileName(sourcePath), fileStream, doProgress);
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="fileName">上传文件名</param>
        /// <param name="sourceStream">读取文件流</param>
        /// <param name="doProgress">执行进度的回调函数, 回调函数返回值如为false则中止上传</param>
        /// <returns>完成上传时返回消息</returns>
        public async Task<string> UploadFileAsync(string message, string fileName, Stream sourceStream, Func<FileChunkInfo, bool> doProgress)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));

            string result = null;
            const int maxChunkSize = 64 * 1024;
            int chunkCount = (int) Math.Ceiling(sourceStream.Length * 1.0 / maxChunkSize);
            for (int i = 1; i <= chunkCount; i++)
            {
                int chunkSize = i < chunkCount ? maxChunkSize : (int) (sourceStream.Length - maxChunkSize * (chunkCount - 1));
                byte[] chunkBody = new byte[chunkSize];
                sourceStream.Seek(maxChunkSize * (i - 1), SeekOrigin.Begin);
                await sourceStream.ReadAsync(chunkBody, 0, chunkSize);
                FileChunkInfo chunkInfo = new FileChunkInfo(fileName, chunkCount, i, chunkSize, maxChunkSize, chunkBody);
                if (doProgress == null || doProgress(chunkInfo))
                    result = await UploadFileChunkAsync(message, chunkInfo);
                else
                    return await UploadFileChunkAsync(message, new FileChunkInfo(fileName));
            }

            return result;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="message">上传消息</param>
        /// <param name="chunkInfo">上传文件块信息</param>
        /// <returns>完成上传时返回消息</returns>
        public async Task<string> UploadFileChunkAsync(string message, FileChunkInfo chunkInfo)
        {
            if (chunkInfo == null)
                throw new ArgumentNullException(nameof(chunkInfo));

            using (MultipartFormDataContent formDataContent = new MultipartFormDataContent())
            {
                formDataContent.Add(new StringContent(message, Encoding.UTF8), "message");
                formDataContent.Add(new StringContent(Utilities.JsonSerialize(chunkInfo), Encoding.UTF8), "chunkInfo");
                return await CallAsync<string>(HttpMethod.Put, ApiConfig.ApiInoutFilePath, formDataContent);
            }
        }

        #endregion

        #region Log

        /// <summary>
        /// 保存错误日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="error">错误</param>
        public async Task SaveEventLogAsync(string message, Exception error = null)
        {
            await SaveEventLogAsync(new StackTrace().GetFrame(1).GetMethod(), message, error);
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="method">函数的信息</param>
        /// <param name="message">消息</param>
        /// <param name="error">错误</param>
        public async Task SaveEventLogAsync(MethodBase method, string message, Exception error = null)
        {
            if (!await SaveEventLogAsync(new Phenix.Core.Log.EventInfo(await GetSequenceAsync(), DateTime.Now,
                method != null ? (method.ReflectedType ?? method.DeclaringType).FullName : null,
                method != null ? method.Name : null,
                message,
                error != null ? error.GetType().FullName : null,
                error != null ? AppRun.GetErrorMessage(error) : null,
                Identity.CurrentIdentity != null ? Identity.CurrentIdentity.User.Name : null, NetConfig.LocalAddress)))
                Phenix.Core.Log.EventLog.SaveLocal(method, message, error);
        }

        /// <summary>
        /// 保存对象日志
        /// </summary>
        /// <param name="info">事件资料</param>
        public async Task<bool> SaveEventLogAsync(Phenix.Core.Log.EventInfo info)
        {
            return await CallAsync<bool>(HttpMethod.Post, ApiConfig.ApiLogEventLogPath, info);
        }

        #endregion

        #region Call

        private string BuildUri(string path, NameValue[] paramValues)
        {
            if (paramValues != null && paramValues.Length > 0)
            {
                StringBuilder result = new StringBuilder();
                foreach (NameValue item in paramValues)
                    result.AppendFormat("{0}={1}&", item.Name, item.Value);
                return String.Format("{0}?{1}", path, result.Remove(result.Length - 1, 1));
            }

            return path;
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="paramValues">参数值</param>
        /// <returns>返回对象</returns>
        public async Task<T> CallAsync<T>(HttpMethod method, string path, params NameValue[] paramValues)
        {
            return await CallAsync<T>(method, path, null, false, false, paramValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="decryptResult">是否解密返回数据（服务端的控制器代码请用Encrypt(result)加密）</param>
        /// <param name="paramValues">参数值</param>
        /// <returns>返回对象</returns>
        public async Task<T> CallAsync<T>(HttpMethod method, string path, bool decryptResult, params NameValue[] paramValues)
        {
            return await CallAsync<T>(method, path, null, false, decryptResult, paramValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="data">上传数据</param>
        /// <param name="paramValues">参数值</param>
        /// <returns>返回对象</returns>
        public async Task<T> CallAsync<T>(HttpMethod method, string path, object data, params NameValue[] paramValues)
        {
            return await CallAsync<T>(method, path, data, false, false, paramValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="data">上传数据</param>
        /// <param name="encryptData">是否加密上传数据（服务端的控制器代码请用Request.ReadBodyXXX(true)解密）</param>
        /// <param name="paramValues">参数值</param>
        /// <returns>返回对象</returns>
        public async Task<T> CallAsync<T>(HttpMethod method, string path, object data, bool encryptData, params NameValue[] paramValues)
        {
            return await CallAsync<T>(method, path, data, encryptData, false, paramValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="data">上传数据</param>
        /// <param name="encryptData">是否加密上传数据（服务端的控制器代码请用Request.ReadBodyXXX(true)解密）(如果data是HttpContent则忽略)</param>
        /// <param name="decryptResult">是否解密返回数据（服务端的控制器代码请用Encrypt(result)加密）</param>
        /// <param name="paramValues">参数值</param>
        /// <returns>返回对象</returns>
        public async Task<T> CallAsync<T>(HttpMethod method, string path, object data, bool encryptData, bool decryptResult, params NameValue[] paramValues)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (Identity == null)
                throw new AuthenticationException();
            if (!Identity.IsAuthenticated)
                throw new UserVerifyException();

            using (HttpRequestMessage request = new HttpRequestMessage(method, BuildUri(path, paramValues)))
            {
                if (data != null)
                    if (data is HttpContent httpContent)
                        request.Content = httpContent;
                    else
                    {
                        if (data is NameValue nameValue)
                            data = NameValue.ToDictionary(nameValue);
                        request.Content = new StringContent(encryptData ? Identity.User.Encrypt(Utilities.JsonSerialize(data)) : Utilities.JsonSerialize(data), Encoding.UTF8);
                    }

                using (HttpResponseMessage response = await SendAsync(request))
                {
                    await response.ThrowIfFailedAsync();
                    return Utilities.JsonDeserialize<T>(decryptResult ? Identity.User.Decrypt(await response.Content.ReadAsStringAsync()) : await response.Content.ReadAsStringAsync());
                }
            }
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="paramValues">参数值</param>
        public async Task CallAsync(HttpMethod method, string path, params NameValue[] paramValues)
        {
            await CallAsync(method, path, null, false, paramValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="data">上传数据</param>
        /// <param name="paramValues">参数值</param>
        public async Task CallAsync(HttpMethod method, string path, object data, params NameValue[] paramValues)
        {
            await CallAsync(method, path, data, false, paramValues);
        }

        /// <summary>
        /// 呼叫
        /// </summary>
        /// <param name="method">HttpMethod</param>
        /// <param name="path">资源路径(比如"/api/security/myself")</param>
        /// <param name="data">上传数据</param>
        /// <param name="encryptData">是否加密上传数据（服务端的控制器代码请用Request.ReadBodyXXX(true)解密）(如果data是HttpContent则忽略)</param>
        /// <param name="paramValues">参数值</param>
        public async Task CallAsync(HttpMethod method, string path, object data, bool encryptData, params NameValue[] paramValues)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (Identity == null)
                throw new AuthenticationException();
            if (!Identity.IsAuthenticated)
                throw new UserVerifyException();

            using (HttpRequestMessage request = new HttpRequestMessage(method, BuildUri(path, paramValues)))
            {
                if (data != null)
                    if (data is HttpContent httpContent)
                        request.Content = httpContent;
                    else
                    {
                        if (data is NameValue[] nameValues)
                            data = NameValue.ToDictionary(nameValues);
                        request.Content = new StringContent(encryptData ? Identity.User.Encrypt(Utilities.JsonSerialize(data)) : Utilities.JsonSerialize(data), Encoding.UTF8);
                    }

                using (HttpResponseMessage response = await SendAsync(request))
                {
                    await response.ThrowIfFailedAsync();
                }
            }
        }

        #endregion

        #endregion
    }
}
using System;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Phenix.Core.Reflection;
using Phenix.Core.Security;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// HttpRequest扩展
    /// </summary>
    public static class HttpRequestExtension
    {
        /// <summary>
        /// 获取远端IP地址
        /// 前置条件：在Startup里应配置和使用转接头中间件（代理服务器和负载均衡器）
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <returns>远端IP地址</returns>
        public static string GetRemoteAddress(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string result = request.Headers["X-Forwarded-For"].FirstOrDefault() ?? request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            if (String.IsNullOrEmpty(result) && request.HttpContext.Connection.RemoteIpAddress != null)
                result = request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            return result;
        }

        /// <summary>
        /// 获取本地IP地址和端口
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <returns>本地IP地址</returns>
        public static string GetLocalAddressPort(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return String.Format("{0}:{1}", Phenix.Core.Net.NetConfig.LocalAddress, request.HttpContext.Connection.LocalPort);
        }

        /// <summary>
        /// 获取区域性名称
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <returns>区域性名称</returns>
        public static string GetAcceptLanguage(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string result = request.Headers["Accept-Language"].FirstOrDefault();
            if (result != null)
            {
                int i = result.IndexOf(',');
                if (i > 0)
                    return result.Substring(0, i);
            }

            return result;
        }

        /// <summary>
        /// 获取报文主体
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <returns>报文主体</returns>
        public static Stream ReadBody(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Stream result = request.Body;
            if (!result.CanRead)
                return null;

            if (!result.CanSeek)
            {
                request.EnableBuffering(); // 将HttpRequestStream转换为FileBufferingReadStream
                result = request.Body;
            }

            result.Position = 0;
            return result;
        }

        /// <summary>
        /// 获取报文主体
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <param name="decrypt">需要解密</param>
        /// <returns>报文主体</returns>
        public static async Task<T> ReadBodyAsync<T>(this HttpRequest request, bool decrypt = false)
        {
            return Utilities.JsonDeserialize<T>(await ReadBodyAsStringAsync(request, decrypt));
        }

        /// <summary>
        /// 获取报文主体
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <param name="decrypt">需要解密</param>
        /// <returns>报文主体</returns>
        public static async Task<string> ReadBodyAsStringAsync(this HttpRequest request, bool decrypt = false)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            using (StreamReader streamReader = new StreamReader(ReadBody(request), Encoding.UTF8))
            {
                if (decrypt)
                {
                    if (Principal.CurrentIdentity == null)
                        throw new AuthenticationException();
                    return await Principal.CurrentIdentity.Decrypt(await streamReader.ReadToEndAsync());
                }

                return await streamReader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// 获取报文主体
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <param name="decrypt">需要解密</param>
        /// <returns>报文主体</returns>
        public static async Task<dynamic> ReadBodyAsDynamicAsync(this HttpRequest request, bool decrypt = false)
        {
            return JObject.Parse(await ReadBodyAsStringAsync(request, decrypt));
        }

        /// <summary>
        /// 获取文件块
        /// </summary>
        /// <param name="request">HttpRequest含"chunkBody"文件</param>
        /// <returns>文件块</returns>
        public static async Task<byte[]> ReadBodyAsFileChunkAsync(this HttpRequest request)
        {
            return await ReadBodyAsFileChunkAsync(request, CancellationToken.None);
        }

        /// <summary>
        /// 获取文件块
        /// </summary>
        /// <param name="request">HttpRequest含"chunkBody"文件</param>
        /// <param name="cancellationToken">取消</param>
        /// <returns>文件块</returns>
        public static async Task<byte[]> ReadBodyAsFileChunkAsync(this HttpRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            IFormFile formFile = request.Form.Files.GetFile("chunkBody");
            if (formFile == null)
                return null;

            await using (MemoryStream stream = new MemoryStream())
            {
                await formFile.CopyToAsync(stream, cancellationToken);
                await stream.FlushAsync(cancellationToken);
                return stream.ToArray();
            }
        }
    }
}
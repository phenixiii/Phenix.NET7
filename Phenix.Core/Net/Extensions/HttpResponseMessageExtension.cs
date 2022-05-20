using System.Security;
using System.Security.Authentication;
using System.Threading.Tasks;
using Phenix.Core.Reflection;

namespace System.Net.Http
{
    /// <summary>
    /// HttpResponseMessage扩展
    /// </summary>
    public static class HttpResponseMessageExtension
    {
        /// <summary>
        /// 如果调用失败就抛出异常
        /// </summary>
        /// <param name="message">HttpResponseMessage</param>
        public static async Task ThrowIfFailedAsync(this HttpResponseMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if ((int) message.StatusCode < 300)
                return;

            switch (message.StatusCode)
            {
                case HttpStatusCode.BadRequest: //等效于 HTTP 状态 400 -> 表示API的消费者发送到服务器的请求是错误的
                    throw new ArgumentException(await message.Content.ReadAsStringAsync());
                case HttpStatusCode.Unauthorized: //等效于 HTTP 状态 401 -> 表示用户没有认证，无法进行操作
                    throw new AuthenticationException(await message.Content.ReadAsStringAsync());
                case HttpStatusCode.Forbidden: //等效于 HTTP 状态 403 -> 表示用户验证成功，但是该用户仍然无法访问该资源
                    throw new SecurityException(await message.Content.ReadAsStringAsync());
                case HttpStatusCode.Conflict: //等效于 HTTP 状态 409 -> 服务器在完成请求时发生冲突
                    throw new Phenix.Core.Data.Validation.ValidationException(Utilities.JsonDeserialize<Phenix.Core.Data.Validation.ValidationMessage>(await message.Content.ReadAsStringAsync()));
                case HttpStatusCode.NotImplemented: //等效于 HTTP 状态 501 -> 服务器不具备完成请求的功能 
                    throw new NotSupportedException(await message.Content.ReadAsStringAsync());
                case HttpStatusCode.InternalServerError: //等效于 HTTP 状态 500 -> 服务器遇到错误，无法完成请求
                    throw new SystemException(await message.Content.ReadAsStringAsync());
                default:
                    throw new Exception(await message.Content.ReadAsStringAsync());
            }
        }
    }
}
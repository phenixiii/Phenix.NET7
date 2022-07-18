using System;
using System.Net;
using System.Security;
using System.Security.Authentication;
using System.Threading.Tasks;
using Phenix.Core;
using Phenix.Core.Reflection;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// HttpResponse扩展
    /// </summary>
    public static class HttpResponseExtension
    {

        /// <summary>
        /// 打包错误消息
        /// </summary>
        /// <param name="response">HttpResponse</param>
        /// <param name="error">错误消息</param>
        public static async Task PackAsync(this HttpResponse response, Exception error)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (error == null)
            {
                response.StatusCode = (int)HttpStatusCode.OK; //等效于 HTTP 状态 200 -> 成功
                return;
            }

            response.Headers["Access-Control-Allow-Origin"] = "*";

            switch (error)
            {
                case ArgumentException _:
                    response.StatusCode = (int)HttpStatusCode.BadRequest; //等效于 HTTP 状态 400 -> 表示API的消费者发送到服务器的请求是错误的
                    break;
                case AuthenticationException _:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized; //等效于 HTTP 状态 401 -> 表示用户没有认证，无法进行操作
                    break;
                case SecurityException _:
                    response.StatusCode = (int)HttpStatusCode.Forbidden; //等效于 HTTP 状态 403 -> 表示用户验证成功，但是该用户仍然无法访问该资源
                    break;
                case System.ComponentModel.DataAnnotations.ValidationException _:
                case InvalidOperationException _:
                    response.StatusCode = (int)HttpStatusCode.Conflict; //等效于 HTTP 状态 409 -> 服务器在完成请求时发生冲突
                    await response.WriteAsync(error is Phenix.Core.Data.Validation.ValidationException validationException
                        ? Utilities.JsonSerialize(validationException.ValidationMessage)
                        : Utilities.JsonSerialize(new Phenix.Core.Data.Validation.ValidationMessage(null, -1, error.Message)));
                    return;
                case NotSupportedException _:
                case NotImplementedException _:
                    response.StatusCode = (int)HttpStatusCode.NotImplemented; //等效于 HTTP 状态 501 -> 服务器不具备完成请求的功能
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError; //等效于 HTTP 状态 500 -> 服务器遇到错误，无法完成请求
                    break;
            }

            await response.WriteAsync(AppRun.GetErrorHint(error));
        }
    }
}
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Phenix.Core;
using Phenix.Core.Log;

namespace Phenix.Services.Host.Mvc
{
    /// <summary>
    /// 异常处理中间件
    /// 
    /// 拦截异常并转译成 context.Response.StatusCode
    /// 异常详情见报文体：
    /// System.ArgumentException/System.InvalidOperationException 转译为 400 BadRequest
    /// System.Security.Authentication.AuthenticationException 转译为 401 Unauthorized
    /// System.Security.SecurityException 转译为 403 Forbidden
    /// System.ComponentModel.DataAnnotations.ValidationException 转译为 409 Conflict
    /// System.NotSupportedException/System.NotImplementedException 转译为 501 NotImplemented
    /// 除以上之外的异常都转译为 500 InternalServerError
    /// </summary>
    public sealed class ExceptionHandlerMiddleware
    {
        /// <summary>
        /// 异常处理中间件
        /// </summary>
        /// <param name="next">下一个中间件</param>
        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #region 属性

        private readonly RequestDelegate _next;

        #endregion

        #region 方法

        /// <summary>
        /// 动态执行
        /// </summary>
        /// <param name="context">上下文</param>
        public async Task InvokeAsync(HttpContext context)
        {
            DateTime dateTime = DateTime.Now;
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await context.Response.PackAsync(ex);
            }
            finally
            {
                if (AppRun.Debugging)
                    await Task.Run(() => EventLog.SaveLocal(String.Format("{0} {1} {2}({3}) take {4} millisecond.",
                        context.Request.Path, context.Request.QueryString, context.Request.Method, context.Request.ContentType,
                        DateTime.Now.Subtract(dateTime).TotalMilliseconds.ToString(CultureInfo.InvariantCulture))));
            }
        }

        #endregion
    }
}
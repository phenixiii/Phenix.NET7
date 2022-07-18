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

                if (AppRun.Debugging || DateTime.Now.Subtract(dateTime).Seconds > 3)
                    LogHelper.Debug("{@Context} consume time {@TotalMilliseconds} ms",
                        new
                        {
                            Path = context.Request.Path,
                            QueryString = context.Request.QueryString,
                            Method = context.Request.Method,
                            ContentType = context.Request.ContentType,
                            StatusCode = context.Response.StatusCode,
                        },
                        DateTime.Now.Subtract(dateTime).TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "{@Context} consume time {@TotalMilliseconds} ms",
                    new
                    {
                        Path = context.Request.Path,
                        QueryString = context.Request.QueryString,
                        Method = context.Request.Method,
                        ContentType = context.Request.ContentType,
                        StatusCode = context.Response.StatusCode,
                    },
                    DateTime.Now.Subtract(dateTime).TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                await context.Response.PackAsync(ex);
            }
        }

        #endregion
    }
}
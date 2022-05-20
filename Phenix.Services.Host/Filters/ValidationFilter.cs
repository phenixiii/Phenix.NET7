using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Phenix.Core.Data.Validation;

namespace Phenix.Services.Host.Filters
{
    /// <summary>
    /// 数据验证过滤器标签
    /// </summary>
    public class ValidationFilter : IActionFilter
    {
        /// <summary>
        /// 处理数据验证
        /// </summary>
        /// <param name="context">上下文</param>
        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                List<ValidationMessage> errors = new List<ValidationMessage>(context.ModelState.Count);
                foreach (KeyValuePair<string, ModelStateEntry> modelState in context.ModelState)
                {
                    string hint = null;
                    foreach (ModelError error in modelState.Value.Errors)
                    {
                        if (!String.IsNullOrEmpty(hint))
                            hint = hint + "|";
                        hint = hint + error.ErrorMessage;
                    }

                    errors.Add(new ValidationMessage(modelState.Key, StatusCodes.Status400BadRequest, hint));
                }

                context.Result = new BadRequestObjectResult(new ValidationResult(StatusCodes.Status400BadRequest, errors.ToArray()));
            }
        }

        /// <summary>
        /// 处理数据验证
        /// </summary>
        /// <param name="context">上下文</param>
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}

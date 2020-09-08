using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Net.Filters;
using Phenix.Core.Reflection;

namespace Phenix.Services.Plugin.Api.Service
{
    /// <summary>
    /// 门户控制器
    /// </summary>
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiServicePortalPath)]
    [ApiController]
    public sealed class PortalController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 提交
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<object> Post(string nameSpace, string className, string methodName)
        {
            string commandName = String.Format("{0}.{1}", nameSpace, className);
            Type commandType = Utilities.LoadType(commandName);
            if (commandType == null)
                throw new InvalidOperationException(commandName + "在服务端未定义");
            if (!commandType.IsPublic)
                throw new InvalidOperationException(commandName + "在服务端未公开");

            Dictionary<string, object> body = Utilities.JsonDeserialize<Dictionary<string, object>>(await Request.ReadBodyAsStringAsync());
            foreach (MethodInfo methodInfo in commandType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public))
                if (String.CompareOrdinal(methodInfo.Name, methodName) == 0)
                {
                    ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                    if (parameterInfos.Length == body.Count)
                    {
                        List<object> args = new List<object>(parameterInfos.Length);
                        try
                        {
                            foreach (ParameterInfo parameterInfo in parameterInfos)
                                if (body.TryGetValue(parameterInfo.Name, out object value))
                                    args.Add(Utilities.ChangeType(value, parameterInfo.ParameterType));
                        }
                        catch
                        {
                            continue;
                        }

                        if (args.Count != parameterInfos.Length)
                            continue;
                        ControllerRole controllerRole = ControllerRole.Fetch(commandType.GetTypeInfo(), methodInfo);
                        await controllerRole.CheckValidityAsync(User.Identity, Request.HttpContext);
                        return controllerRole.MethodInfo.Invoke(null, args.ToArray());
                    }
                }

            throw new NotImplementedException(String.Format("{0}.{1}.{2}", nameSpace, className, methodName));
        }
    }
}
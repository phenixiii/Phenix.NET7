using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Data;
using Phenix.Services.Contract;

namespace Phenix.Services.Plugin.Data
{
    /// <summary>
    /// 64位增量控制器
    /// </summary>
    [Route(WebApiConfig.DataIncrementPath)]
    [ApiController]
    public sealed class IncrementController : Phenix.Net.Api.ControllerBase
    {
        // phAjax.getIncrement()
        /// <summary>
        /// 获取64位增量值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="initialValue">初值</param>
        /// <returns>下一个值</returns>
        [Authorize]
        [HttpGet]
        public long GetNext(string key, long initialValue)
        {
            return Database.Default.Increment.GetNext(key, initialValue);
        }
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Data;

namespace Phenix.Services.Plugin.Api.Data
{
    /// <summary>
    /// 64位增量
    /// </summary>
    [EnableCors]
    [Route(Phenix.Core.Net.Api.ApiConfig.ApiDataIncrementPath)]
    [ApiController]
    public sealed class IncrementController : Phenix.Core.Net.Api.ControllerBase
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


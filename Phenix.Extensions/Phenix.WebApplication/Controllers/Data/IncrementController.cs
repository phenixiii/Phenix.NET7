using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Net;

namespace Phenix.WebApplication.Controllers.Data
{
    /// <summary>
    /// 64位增量
    /// </summary>
    [EnableCors]
    [Route(NetConfig.DataIncrementPath)]
    [ApiController]
    public sealed class IncrementController : Phenix.Core.Net.ControllerBase
    {
        // GET: /api/data/increment?key=文牒&initialValue=1
        // phAjax.getIncrement()
        /// <summary>
        /// 获取64位增量值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="initialValue">初值</param>
        /// <returns>下一个值</returns>
        [Authorize]
        [HttpGet]
        public long Get(string key, long initialValue)
        {
            return Phenix.Core.Data.Increment.GetNext(key, initialValue);
        }
    }
}


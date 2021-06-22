using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Data;

namespace Phenix.Services.Plugin.Data
{
    /// <summary>
    /// 64位序号控制器
    /// </summary>
    [Route(ApiConfig.ApiDataSequencePath)]
    [ApiController]
    public sealed class SequenceController : Phenix.Core.Net.Api.ControllerBase
    {
        // phAjax.getSequence()
        /// <summary>
        /// 获取64位序号
        /// </summary>
        /// <returns>64位序号</returns>
        [Authorize]
        [HttpGet]
        public long Get()
        {
            return Database.Default.Sequence.Value;
        }
    }
}

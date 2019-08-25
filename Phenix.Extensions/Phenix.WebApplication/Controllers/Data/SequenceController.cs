using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Net;

namespace Phenix.WebApplication.Controllers.Data
{
    /// <summary>
    /// 64位序号
    /// </summary>
    [EnableCors]
    [Route(NetConfig.DataSequencePath)]
    [ApiController]
    public sealed class SequenceController : Phenix.Core.Net.ControllerBase
    {
        // GET: /api/data/sequence
        // phAjax.getSequence()
        /// <summary>
        /// 获取64位序号
        /// </summary>
        /// <returns>64位序号</returns>
        [Authorize]
        [HttpGet]
        public long Get()
        {
            return Phenix.Core.Data.Sequence.Value;
        }
    }
}

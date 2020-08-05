using System.Threading.Tasks;
using Demo.IDOS.Plugin.Actor.OnlineBooking;
using Demo.IDOS.Plugin.Business.OnlineBooking;
using Demo.IDOS.Plugin.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phenix.Actor;
using Phenix.Core.Data.Schema;

namespace Demo.IDOS.Plugin.Api.OnlineBooking
{
    /// <summary>
    /// 出库预约单
    /// </summary>
    [Route(ApiConfig.ApiOnlineBookingOutBookingNotePath)]
    [ApiController]
    public sealed class OutBookingNoteController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取出库预约单
        /// </summary>
        /// <param name="depotId">仓库ID</param>
        /// <param name="bookingNumber">预约单号</param>
        /// <returns>出库预约单</returns>
        [Authorize(ApiRoles.BookingPerson)]
        [HttpGet]
        public async Task<DobOutBookingNote> Get(long depotId, string bookingNumber)
        {
            DobOutBookingNote result = await ClusterClient.Default.GetGrain<IOutBookingGrain>(depotId).GetNote(bookingNumber);
            await AuthorizationFilters.CheckCustomerUserValidity(User.Identity, result.CmId);
            return result;
        }

        /// <summary>
        /// 新增出库预约单
        /// </summary>
        /// <param name="note">出库预约单</param>
        /// <returns>出库预约单</returns>
        [Authorize(ApiRoles.BookingPerson)]
        [HttpPost]
        public async Task<DobOutBookingNote> Post([FromBody] DobOutBookingNote note)
        {
            await AuthorizationFilters.CheckCustomerUserValidity(User.Identity, note.CmId);
            return await ClusterClient.Default.GetGrain<IOutBookingGrain>(note.DpId).PostNote(note.FillReservedFields(ExecuteAction.Insert));
        }

        /// <summary>
        /// 更新出库预约单
        /// </summary>
        /// <param name="note">出库预约单</param>
        /// <returns>出库预约单</returns>
        [Authorize(ApiRoles.BookingPerson)]
        [HttpPatch]
        public async Task<DobOutBookingNote> Patch([FromBody] DobOutBookingNote note)
        {
            await AuthorizationFilters.CheckCustomerUserValidity(User.Identity, note.CmId);
            return await ClusterClient.Default.GetGrain<IOutBookingGrain>(note.DpId).PatchNote(note.FillReservedFields(ExecuteAction.Update));
        }
    }
}

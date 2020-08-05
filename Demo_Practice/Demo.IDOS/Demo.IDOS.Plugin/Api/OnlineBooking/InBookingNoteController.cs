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
    /// 入库预约单
    /// </summary>
    [Route(ApiConfig.ApiOnlineBookingInBookingNotePath)]
    [ApiController]
    public sealed class InBookingNoteController : Phenix.Core.Net.Api.ControllerBase
    {
        /// <summary>
        /// 获取入库预约单
        /// </summary>
        /// <param name="depotId">仓库ID</param>
        /// <param name="bookingNumber">预约单号</param>
        /// <returns>入库预约单</returns>
        [Authorize(ApiRoles.BookingPerson)]
        [HttpGet]
        public async Task<DobInBookingNote> Get(long depotId, string bookingNumber)
        {
            DobInBookingNote result = await ClusterClient.Default.GetGrain<IInBookingGrain>(depotId).GetNote(bookingNumber);
            await AuthorizationFilters.CheckCustomerUserValidity(User.Identity, result.CmId);
            return result;
        }

        /// <summary>
        /// 新增入库预约单
        /// </summary>
        /// <param name="note">入库预约单</param>
        /// <returns>入库预约单</returns>
        [Authorize(ApiRoles.BookingPerson)]
        [HttpPost]
        public async Task<DobInBookingNote> Post([FromBody] DobInBookingNote note)
        {
            await AuthorizationFilters.CheckCustomerUserValidity(User.Identity, note.CmId);
            return await ClusterClient.Default.GetGrain<IInBookingGrain>(note.DpId).PostNote(note.FillReservedFields(ExecuteAction.Insert));
        }

        /// <summary>
        /// 更新入库预约单
        /// </summary>
        /// <param name="note">入库预约单</param>
        /// <returns>入库预约单</returns>
        [Authorize(ApiRoles.BookingPerson)]
        [HttpPatch]
        public async Task<DobInBookingNote> Patch([FromBody] DobInBookingNote note)
        {
            await AuthorizationFilters.CheckCustomerUserValidity(User.Identity, note.CmId);
            return await ClusterClient.Default.GetGrain<IInBookingGrain>(note.DpId).PatchNote(note.FillReservedFields(ExecuteAction.Update));
        }
    }
}

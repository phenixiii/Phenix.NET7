using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.OnlineBooking;
using Orleans;

namespace Demo.IDOS.Plugin.Actor.YardOperation
{
    /// <summary>
    /// ASC总控
    /// </summary>
    public interface IASCCoordinateGrain : IGrainWithIntegerKey
    {
        /// <summary>
        /// 分配站台
        /// </summary>
        /// <param name="note">入库预约单</param>
        public Task<string> DistributePlatform(DobInBookingNote note);

        /// <summary>
        /// 分配站台
        /// </summary>
        /// <param name="note">出库预约单</param>
        public Task<string> DistributePlatform(DobOutBookingNote note);
    }
}

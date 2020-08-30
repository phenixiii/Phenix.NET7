using System;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Business.OnlineBooking;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.YardOperation
{
    /// <summary>
    /// ASC总控
    /// </summary>
    public class ASCCoordinateGrain : GrainBase, IASCCoordinateGrain
    {
        #region 方法

        Task<string> IASCCoordinateGrain.DistributePlatform(DobInBookingNote note)
        {
            throw new NotImplementedException();
        }

        Task<string> IASCCoordinateGrain.DistributePlatform(DobOutBookingNote note)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

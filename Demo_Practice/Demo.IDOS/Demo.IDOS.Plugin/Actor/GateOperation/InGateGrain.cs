using System;
using System.Threading.Tasks;
using Demo.IDOS.Plugin.Actor.OnlineBooking;
using Demo.IDOS.Plugin.Actor.YardOperation;
using Demo.IDOS.Plugin.Business.GateOperation;
using Demo.IDOS.Plugin.Business.OnlineBooking;
using Demo.IDOS.Plugin.Rule;
using Phenix.Actor;

namespace Demo.IDOS.Plugin.Actor.GateOperation
{
    /// <summary>
    /// 进场道口
    /// </summary>
    public class InGateGrain : GrainBase, IInGateGrain
    {
        #region 属性

        private IInBookingGrain _inBookingGrain;

        /// <summary>
        /// 在线入库预约
        /// </summary>
        public IInBookingGrain InBookingGrain
        {
            get { return _inBookingGrain ?? (_inBookingGrain = ClusterClient.Default.GetGrain<IInBookingGrain>(Id.Value)); }
        }

        private IOutBookingGrain _outBookingGrain;

        /// <summary>
        /// 在线出库预约
        /// </summary>
        public IOutBookingGrain OutBookingGrain
        {
            get { return _outBookingGrain ?? (_outBookingGrain = ClusterClient.Default.GetGrain<IOutBookingGrain>(Id.Value)); }
        }

        private IASCCoordinateGrain _ASCCoordinateGrain;

        /// <summary>
        /// ASC总控
        /// </summary>
        public IASCCoordinateGrain ASCCoordinateGrain
        {
            get { return _ASCCoordinateGrain ?? (_ASCCoordinateGrain = ClusterClient.Default.GetGrain<IASCCoordinateGrain>(Id.Value)); }
        }

        #endregion

        #region 方法

        private void SaveOperation(OperationType operationType, string licensePlate, string bookingNumber)
        {
            //启动新的任务
            DgoInGateOperation inGateOperation = DgoInGateOperation.New(Database,
                DgoInGateOperation.Set(p => p.DpId, Id)
                    .Set(p => p.GateName, KeyExtension)
                    .Set(p => p.LicensePlate, licensePlate)
                    .Set(p => p.BookingNumber, bookingNumber)
                    .Set(p => p.OperationType, operationType)
                    .Set(p => p.OperationTime, DateTime.Now));
            inGateOperation.InsertSelf();
        }

        Task<string> IInGateGrain.PermitByLicensePlate(string value)
        {
            Task<DobInBookingNote> inTask = Task.Run(() => InBookingGrain.GetNote(null, value));
            Task<DobOutBookingNote> outTask = Task.Run(() => OutBookingGrain.GetNote(null, value));
            Task.WaitAll(inTask, outTask);

            DobInBookingNote inBookingNote = inTask.Result;
            if (inBookingNote != null)
            {
                SaveOperation(OperationType.StockIn, inBookingNote.LicensePlate, inBookingNote.BookingNumber);
                InBookingGrain.OperateNote(inBookingNote.BookingNumber);
                ASCCoordinateGrain.DistributePlatform(inBookingNote);
                return Task.FromResult("请驶入检测区");
            }

            DobOutBookingNote outBookingNote = outTask.Result;
            if (outBookingNote != null)
            {
                SaveOperation(OperationType.StockOut, outBookingNote.LicensePlate, outBookingNote.BookingNumber);
                OutBookingGrain.OperateNote(outBookingNote.BookingNumber);
                ASCCoordinateGrain.DistributePlatform(outBookingNote);
                return Task.FromResult("请驶入站台");
            }

            return Task.FromResult(String.Format("非业务车辆请离场! 如车辆识别({0})有误, 请联系工作人员!", value));
        }

        #endregion
    }
}

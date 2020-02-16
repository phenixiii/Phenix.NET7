using System.Threading.Tasks;
using Demo.InspectionStation.Plugin.Business;
using Orleans;

namespace Demo.InspectionStation.Plugin.Actor
{
    /// <summary>
    /// 作业点Grain接口
    /// </summary>
    public interface IOperationPointGrain : IGrainWithStringKey
    {
        /// <summary>
        /// 获取状态
        /// </summary>
        public Task<OperationPointStatus> GetStatus();

        /// <summary>
        /// 获取磅秤重
        /// </summary>
        public Task<int> GetWeighbridge();

        /// <summary>
        /// 设置磅秤重
        /// </summary>
        /// <param name="value">值</param>
        public Task SetWeighbridge(int value);

        /// <summary>
        /// 磅秤(读取设备)心跳(每10秒至少2次)
        /// </summary>
        public Task WeighbridgeAlive();

        /// <summary>
        /// 获取车牌号
        /// </summary>
        public Task<string> GetLicensePlate();

        /// <summary>
        /// 设置车牌号
        /// </summary>
        /// <param name="value">值</param>
        public Task SetLicensePlate(string value);

        /// <summary>
        /// 车牌(识别设备)心跳(每10秒至少2次)
        /// </summary>
        public Task LicensePlateAlive();

        /// <summary>
        /// 放行
        /// </summary>
        public Task PermitThrough();
    }
}

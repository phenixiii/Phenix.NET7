using System.Threading.Tasks;
using Orleans;

namespace Demo.IDOS.Plugin.Actor.GateOperation
{
    /// <summary>
    /// 进场道口
    /// </summary>
    public interface IInGateGrain : IGrainWithIntegerCompoundKey
    {
        /// <summary>
        /// 车牌号放行
        /// </summary>
        /// <param name="value">值</param>
        public Task<string> PermitByLicensePlate(string value);
    }
}

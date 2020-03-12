using System;

namespace Demo.InspectionStation.Plugin.Business
{
    /// <summary>
    /// 作业点状态
    /// </summary>
    [Serializable]
    public enum OperationPointStatus
    {
        /// <summary>
        /// 脱机
        /// </summary>
        Shutdown = 0,

        /// <summary>
        /// 待机
        /// </summary>
        Standby = 1,

        /// <summary>
        /// 登记
        /// </summary>
        CheckIn = 2,

        /// <summary>
        /// 放行
        /// </summary>
        PermitThrough = 9,
    }
}

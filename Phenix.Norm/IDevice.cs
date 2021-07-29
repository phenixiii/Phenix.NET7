using Phenix.Core.Data;
using Phenix.Core.Data.Expressions;
using Phenix.Core.Data.Model;

namespace Phenix.Norm
{
    /// <summary>
    /// 设备接口
    /// </summary>
    public interface IDevice<T> : IEntity<T>
        where T : IDevice<T>
    {
        #region 属性

        /// <summary>
        /// 设备状态
        /// 仅允许本接口提供的DeviceStatus属性间接更新并持久化（表字段建议命名为DEVICE_STATUS_KEY）
        /// </summary>
        public string DeviceStatusKey { get; }

        /// <summary>
        /// 设备状态
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public DeviceStatus DeviceStatus
        {
            get { return EnumKeyValue.GetEnumFirst<DeviceStatus>(p => p.Key == DeviceStatusKey); }
            set { UpdateSelf(NameValue.Set<T>(p => p.DeviceStatusKey, EnumKeyValue.Fetch(value).Key)); }
        }
        
        #endregion
    }
}
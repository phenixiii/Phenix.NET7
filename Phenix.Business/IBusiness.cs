using Phenix.Mapper;

namespace Phenix.Business
{
    /// <summary>
    /// 业务对象接口
    /// </summary>
    public interface IBusiness : IEntity, ISupportUndo
    {
        #region 属性

        /// <summary>
        /// 更新状态(含从业务对象)
        /// </summary>
        bool IsDirty { get; }

        #endregion
    }
}

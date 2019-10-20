using Phenix.Core.Data.Model;

namespace Phenix.Business
{
    /// <summary>
    /// 业务对象接口
    /// </summary>
    public interface IBusiness : IEntity, ISupportUndo
    {
        #region 属性

        /// <summary>
        /// 根业务对象
        /// </summary>
        IBusiness Root { get; }

        /// <summary>
        /// 主业务对象
        /// </summary>
        IBusiness Master { get; }

        /// <summary>
        /// 更新状态(含从业务对象)
        /// </summary>
        bool IsDirty { get; }

        #endregion

        #region 方法

        #endregion
    }
}

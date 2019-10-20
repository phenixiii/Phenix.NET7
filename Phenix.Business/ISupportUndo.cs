using System.Collections.Generic;

namespace Phenix.Business
{
    /// <summary>
    /// 支持编辑回滚
    /// </summary>
    public interface ISupportUndo
    {
        #region 属性

        /// <summary>
        /// 离线状态
        /// </summary>
        bool IsFetched { get; }

        /// <summary>
        /// 更新状态
        /// </summary>
        bool IsSelfDirty { get; }

        /// <summary>
        /// 新增状态
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// 删除状态
        /// </summary>
        bool IsSelfDeleted { get; }

        /// <summary>
        /// 旧属性值
        /// </summary>
        IDictionary<string, object> OldPropertyValues { get; }

        /// <summary>
        /// 脏属性值
        /// </summary>
        IDictionary<string, bool?> DirtyPropertyNames { get; }

        #endregion

        #region 方法

        /// <summary>
        /// 启动编辑(仅允许编辑非IsSelfDirty状态的对象)
        /// 快照当前数据
        /// </summary>
        void BeginEdit();

        /// <summary>
        /// 撤销编辑(仅允许撤销IsSelfDirty状态的对象)
        /// 恢复快照数据
        /// </summary>
        void CancelEdit();

        /// <summary>
        /// 应用编辑
        /// 丢弃快照数据
        /// </summary>
        void ApplyEdit();

        #endregion
    }
}


using System;

namespace Phenix.Core.Data
{
    /// <summary>
    /// 执行动作
    /// </summary>
    [Flags]
    [Serializable]
    public enum ExecuteAction : int
    {
        /// <summary>
        /// 无 
        /// </summary>
        None = 0,

        /// <summary>
        /// 新增 
        /// </summary>
        Insert = 1,

        /// <summary>
        /// 更新 
        /// </summary>
        Update = 2,

        /// <summary>
        /// 删除
        /// </summary>
        Delete = 4,
    }
}

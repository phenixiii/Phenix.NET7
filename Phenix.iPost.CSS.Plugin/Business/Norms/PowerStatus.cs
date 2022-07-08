using System;

namespace Phenix.iPost.CSS.Plugin.Business.Norms
{
    /// <summary>
    /// 动力状态
    /// </summary>
    [Serializable]
    public enum PowerStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Green,

        /// <summary>
        /// 由人员检查判断是否需要更换电池/加油
        /// </summary>
        Yellow,

        /// <summary>
        /// 需要立刻更换电池/加油
        /// </summary>
        Red,
    }
}

using System;

namespace Phenix.iPost.CSS.Plugin.Business.Property
{
    /// <summary>
    /// 作业位置属性
    /// </summary>
    /// <param name="Site">场地（装卸点/换电站/停车位，全域唯一，即可控生产区域内各类目的地之间都不允许重复）</param>
    /// <param name="Bay">贝位（目的地是装卸点才有意义）</param>
    /// <param name="Lane">车道（目的地是装卸点才有意义）</param>
    [Serializable]
    public readonly record struct OperationLocationProperty(
        string Site,
        string Bay,
        string Lane);
}

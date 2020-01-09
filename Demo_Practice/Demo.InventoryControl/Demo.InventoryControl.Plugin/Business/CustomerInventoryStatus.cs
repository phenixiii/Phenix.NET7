namespace Demo.InventoryControl.Plugin.Business
{
    /// <summary>
    /// 货主库存状态
    public enum CustomerInventoryStatus
    {
        /// <summary>
        /// 已入库
        /// </summary>
        Stored = 0,

        /// <summary>
        /// 已被挑
        /// </summary>
        Picked = 1,

        /// <summary>
        /// 已出库
        /// </summary>
        NotStored = 9,
    }
}

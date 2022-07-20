namespace System.Collections.Generic
{
    /// <summary>
    /// 列表扩展
    /// </summary>
    public static class ListExtension
    {
        #region Add

        /// <summary>
        /// 一次添加项(如果已含则不添加)
        /// </summary>
        /// <param name="infos">内容</param>
        /// <param name="item">项</param>
        public static bool AddOnce<T>(this IList<T> infos, T item)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            if (!infos.Contains(item))
            {
                infos.Add(item);
                return true;
            }

            return false;
        }

        #endregion
    }
}
namespace System.Collections.Generic
{
    /// <summary>
    /// �б���չ
    /// </summary>
    public static class ListExtension
    {
        #region Add

        /// <summary>
        /// һ�������(����Ѻ������)
        /// </summary>
        /// <param name="infos">����</param>
        /// <param name="item">��</param>
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
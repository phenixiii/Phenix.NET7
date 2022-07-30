using System.Threading.Tasks;
using Phenix.Core.Threading;

namespace System.Collections.Generic
{
    /// <summary>
    /// 数据字典扩展
    /// </summary>
    public static class DictionaryExtension
    {
        #region GetValue

        /// <summary>
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="infos">内容</param>
        /// <param name="key">键</param>
        /// <param name="doCreate">如果没有该键, 构建值的函数</param>
        /// <param name="reset">如果找到该键, 是否重置</param>
        /// <returns>如果找到该键, 便会返回与指定的键相关联的值; 否则, 则会执行 doCreate 函数构建构建 item 的值关联到键并返回</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, Func<TValue> doCreate, bool reset = false)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            if (!reset && infos.TryGetValue(key, out TValue result))
                return result;
            return infos.CreateValue(key, doCreate);
        }

        /// <summary>
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="infos">内容</param>
        /// <param name="key">键</param>
        /// <param name="doCreate">如果没有该键, 构建值的函数</param>
        /// <param name="reset">如果找到该键, 是否重置的函数(null代表false)</param>
        /// <returns>如果找到该键, 便会返回与指定的键相关联的值; 否则, 则会执行 doCreate 函数构建构建 item 的值关联到键并返回</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, Func<TValue> doCreate, Func<TValue, bool> reset)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            if (infos.TryGetValue(key, out TValue result))
                if (reset == null || !reset(result))
                    return result;
            return infos.CreateValue(key, doCreate);
        }

        private static TValue CreateValue<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, Func<TValue> doCreate)
        {
            TValue result = doCreate != null ? doCreate() : (TValue)Activator.CreateInstance(typeof(TValue), true);
            infos[key] = result;
            return result;
        }

        /// <summary>
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="infos">内容</param>
        /// <param name="key">键</param>
        /// <param name="doCreate">如果没有该键, 构建值的函数</param>
        /// <param name="reset">如果找到该键, 是否重置</param>
        /// <returns>如果找到该键, 便会返回与指定的键相关联的值; 否则, 则会执行 doCreate 函数构建构建 item 的值关联到键并返回</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, Func<Task<TValue>> doCreate, bool reset = false)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            if (!reset && infos.TryGetValue(key, out TValue result))
                return result;
            return infos.CreateValue(key, doCreate);
        }

        /// <summary>
        /// 获取与指定的键相关联的值
        /// </summary>
        /// <param name="infos">内容</param>
        /// <param name="key">键</param>
        /// <param name="doCreate">如果没有该键, 构建值的函数</param>
        /// <param name="reset">如果找到该键, 是否重置的函数(null代表false)</param>
        /// <returns>如果找到该键, 便会返回与指定的键相关联的值; 否则, 则会执行 doCreate 函数构建构建 item 的值关联到键并返回</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, Func<Task<TValue>> doCreate, Func<TValue, bool> reset)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            if (infos.TryGetValue(key, out TValue result))
                if (reset == null || !reset(result))
                    return result;
            return infos.CreateValue(key, doCreate);
        }

        private static TValue CreateValue<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, Func<Task<TValue>> doCreate)
        {
            TValue result = doCreate != null ? AsyncHelper.RunSync(doCreate) : (TValue)Activator.CreateInstance(typeof(TValue), true);
            infos[key] = result;
            return result;
        }

        #endregion

        #region Add

        /// <summary>
        /// 一次添加项(如果已含则不添加)
        /// </summary>
        /// <param name="infos">内容</param>
        /// <param name="item">项</param>
        public static bool AddOnce<TKey, TValue>(this IDictionary<TKey, TValue> infos, KeyValuePair<TKey, TValue> item)
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

        /// <summary>
        /// 一次将指定的键和值添加到字典中(如果已含则不添加)
        /// </summary>
        /// <param name="infos">内容</param>
        /// <param name="key">键</param>
        /// <param name="value">要添加的元素的值. 对于引用类型, 该值可以为 null</param>
        public static bool AddOnce<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, TValue value)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            if (!infos.ContainsKey(key))
            {
                infos.Add(key, value);
                return true;
            }

            return false;
        }

        #endregion

        #region Remove

        /// <summary>
        /// 移除所指定的键的值
        /// </summary>
        /// <param name="infos">内容</param>
        /// <param name="key">键</param>
        /// <param name="allow">是否允许的函数(null代表true)</param>
        public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, Func<TValue, bool> allow)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            if (infos.TryGetValue(key, out TValue value))
            {
                if (allow != null && !allow(value))
                    return false;
                return infos.Remove(key);
            }

            return false;
        }

        #endregion

        #region Replace

        /// <summary>
        /// 替换值
        /// </summary>
        /// <param name="infos">内容</param>
        /// <param name="key">键</param>
        /// <param name="doReplace">替换值的函数</param>
        /// <param name="doSetIfNotFound">找不到时添加值</param>
        public static void ReplaceValue<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, Func<TValue, TValue> doReplace, Func<TValue> doSetIfNotFound = null)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));
            if (doReplace == null)
                throw new ArgumentNullException(nameof(doReplace));

            TValue result;
            if (infos.TryGetValue(key, out TValue value))
                result = doReplace(value);
            else if (doSetIfNotFound != null)
                result = doSetIfNotFound();
            else
                return;
            infos[key] = result;
        }

        #endregion
    }
}
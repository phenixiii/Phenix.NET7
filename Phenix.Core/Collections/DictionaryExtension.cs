using System.Threading.Tasks;
using Phenix.Core.Threading;

namespace System.Collections.Generic
{
    /// <summary>
    /// �����ֵ���չ
    /// </summary>
    public static class DictionaryExtension
    {
        #region GetValue

        /// <summary>
        /// ��ȡ��ָ���ļ��������ֵ
        /// </summary>
        /// <param name="infos">����</param>
        /// <param name="key">��</param>
        /// <param name="doCreate">���û�иü�, ����ֵ�ĺ���</param>
        /// <param name="reset">����ҵ��ü�, �Ƿ�����</param>
        /// <returns>����ҵ��ü�, ��᷵����ָ���ļ��������ֵ; ����, ���ִ�� doCreate ������������ item ��ֵ��������������</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, Func<TValue> doCreate, bool reset = false)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            if (!reset && infos.TryGetValue(key, out TValue result))
                return result;
            return infos.CreateValue(key, doCreate);
        }

        /// <summary>
        /// ��ȡ��ָ���ļ��������ֵ
        /// </summary>
        /// <param name="infos">����</param>
        /// <param name="key">��</param>
        /// <param name="doCreate">���û�иü�, ����ֵ�ĺ���</param>
        /// <param name="reset">����ҵ��ü�, �Ƿ����õĺ���(null����false)</param>
        /// <returns>����ҵ��ü�, ��᷵����ָ���ļ��������ֵ; ����, ���ִ�� doCreate ������������ item ��ֵ��������������</returns>
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
        /// ��ȡ��ָ���ļ��������ֵ
        /// </summary>
        /// <param name="infos">����</param>
        /// <param name="key">��</param>
        /// <param name="doCreate">���û�иü�, ����ֵ�ĺ���</param>
        /// <param name="reset">����ҵ��ü�, �Ƿ�����</param>
        /// <returns>����ҵ��ü�, ��᷵����ָ���ļ��������ֵ; ����, ���ִ�� doCreate ������������ item ��ֵ��������������</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> infos, TKey key, Func<Task<TValue>> doCreate, bool reset = false)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            if (!reset && infos.TryGetValue(key, out TValue result))
                return result;
            return infos.CreateValue(key, doCreate);
        }

        /// <summary>
        /// ��ȡ��ָ���ļ��������ֵ
        /// </summary>
        /// <param name="infos">����</param>
        /// <param name="key">��</param>
        /// <param name="doCreate">���û�иü�, ����ֵ�ĺ���</param>
        /// <param name="reset">����ҵ��ü�, �Ƿ����õĺ���(null����false)</param>
        /// <returns>����ҵ��ü�, ��᷵����ָ���ļ��������ֵ; ����, ���ִ�� doCreate ������������ item ��ֵ��������������</returns>
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
        /// һ�������(����Ѻ������)
        /// </summary>
        /// <param name="infos">����</param>
        /// <param name="item">��</param>
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
        /// һ�ν�ָ���ļ���ֵ��ӵ��ֵ���(����Ѻ������)
        /// </summary>
        /// <param name="infos">����</param>
        /// <param name="key">��</param>
        /// <param name="value">Ҫ��ӵ�Ԫ�ص�ֵ. ������������, ��ֵ����Ϊ null</param>
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
        /// �Ƴ���ָ���ļ���ֵ
        /// </summary>
        /// <param name="infos">����</param>
        /// <param name="key">��</param>
        /// <param name="allow">�Ƿ�����ĺ���(null����true)</param>
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
        /// �滻ֵ
        /// </summary>
        /// <param name="infos">����</param>
        /// <param name="key">��</param>
        /// <param name="doReplace">�滻ֵ�ĺ���</param>
        /// <param name="doSetIfNotFound">�Ҳ���ʱ���ֵ</param>
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
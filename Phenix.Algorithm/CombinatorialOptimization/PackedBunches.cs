using System.Collections.Generic;

namespace Phenix.Algorithm.CombinatorialOptimization
{
    /// <summary>
    /// 被打包的集束物品
    /// </summary>
    public class PackedBunches
    {
        internal PackedBunches(int size, IList<IGoods> value)
        {
            _size = size;
            _value = new List<IGoods>(value);
        }

        #region 属性

        private int _size;

        /// <summary>
        /// 规格
        /// </summary>
        public int Size
        {
            get { return _size; }
        }

        private readonly List<IGoods> _value;

        /// <summary>
        /// 内容
        /// </summary>
        public IList<IGoods> Value
        {
            get { return _value.AsReadOnly(); }
        }
        
        /// <summary>
        /// 原子级内容(拆解为最小粒度的物品)
        /// </summary>
        public IList<IGoods> AtomicValue
        {
            get { return Expansion(_value); }
        }

        #endregion

        #region 方法

        private static IList<IGoods> Expansion(IList<IGoods> value)
        {
            IList<IGoods> result = new List<IGoods>();
            Expansion(value, ref result);
            return result;
        }

        private static void Expansion(IList<IGoods> value, ref IList<IGoods> result)
        {
            foreach (IGoods item in value)
                if (item is IGoodsBunch goodsGroup)
                {
                    if (goodsGroup.Items != null)
                        Expansion(goodsGroup.Items, ref result);
                }
                else
                    result.Add(item);
        }

        internal void AddRange(PackedBunches packedInfo)
        {
            if (packedInfo != null)
            {
                _size = _size + packedInfo._size;
                _value.AddRange(packedInfo._value);
            }
        }

        #endregion
    }
}
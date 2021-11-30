using Phenix.Algorithm.CombinatorialOptimization;

namespace Demo
{
    public class Goods : IGoods
    {
        public Goods(long index, int weight, int value)
        {
            _index = index;
            _weight = weight;
            _value = value;
        }

        #region 属性

        private readonly long _index;

        public long Index
        {
            get { return _index; }
        }

        private readonly int _weight;

        public int Weight
        {
            get { return _weight; }
        }

        int IGoods.Size
        {
            get { return Weight; }
        }

        private readonly int _value;

        public int Value
        {
            get { return _value; }
        }

        int IGoods.Value
        {
            get { return Value; }
        }

        #endregion
    }
}
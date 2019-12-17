using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Phenix.Algorithm.KnapsackProblem
{
    /// <summary>
    /// 0-1背包问题的动态规划算法
    ///
    /// 背包问题：给定一物品(/组)队列，每件物品(/组)都有自己的规格和价值，在限定的总规格(/范围)内，我们如何选择，才能使得放入背包的物品(/组)总价值最高。
    /// 0-1背包问题：这是最基本的背包问题，每个物品(/组)最多只能放一次。
    /// </summary>
    public class ZeroOneDynamicProgramming
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="goodsList">物品(/组)队列</param>
        public ZeroOneDynamicProgramming(IList<IGoods> goodsList)
        {
            _goodsList = goodsList;
        }

        #region 属性

        private const int LIMIT_SIZE_STEP = 100000;
        private static int _limitSizeTryMaximum = LIMIT_SIZE_STEP * 2;

        /// <summary>
        /// 尝试运算极限范围(小于40000)
        /// 缺省为20000
        /// </summary>
        public static int LimitSizeTryMaximum
        {
            get { return _limitSizeTryMaximum; }
            set { _limitSizeTryMaximum = value <= LIMIT_SIZE_STEP * 4 ? value : LIMIT_SIZE_STEP * 2; }
        }

        private readonly IList<IGoods> _goodsList;

        /// <summary>
        /// 物品(/组)队列
        /// </summary>
        public IList<IGoods> GoodsList
        {
            get { return _goodsList; }
        }

        /// <summary>
        /// 待打包的物品队列(已被拆解为最小粒度的物品)
        /// </summary>
        public IList<IGoods> PackingGoodsList
        {
            get { return GetLeafGoodsList(_goodsList, false); }
        }

        private IList<IGoods> _packedGoodsList;

        /// <summary>
        /// 被打包的物品队列(已被拆解为最小粒度的物品)
        /// </summary>
        public IList<IGoods> PackedGoodsList
        {
            get { return _packedGoodsList; }
        }

        private int _packedSize;

        /// <summary>
        /// 被打包物品的规格
        /// </summary>
        public int PackedSize
        {
            get { return _packedSize; }
        }

        private int _packedValue;

        /// <summary>
        /// 被打包物品的价值
        /// </summary>
        public int PackedValue
        {
            get { return _packedValue; }
        }

        private int _subPackedValue;

        /// <summary>
        /// 被打包零散物品的价值
        /// </summary>
        public int SubPackedValue
        {
            get { return _subPackedValue; }
        }

        /// <summary>
        /// 是否打包成功
        /// </summary>
        public bool Packed
        {
            get { return _packedGoodsList != null && _packedGoodsList.Count > 0; }
        }

        #endregion

        /// <summary>
        /// 打包内容
        /// </summary>
        private class PackedInfo
        {
            public PackedInfo(IList<IGoods> packedGoodsList)
            {
                _packedGoodsList = new List<IGoods>(packedGoodsList);
                _packedValue = 0;
                _subPackedValue = 0;
                foreach (IGoods item in packedGoodsList)
                {
                    _packedSize = _packedSize + item.Size;
                    if (item is IGoodsGroup)
                        _packedValue = _packedValue + item.Value;
                    else
                        _subPackedValue = _subPackedValue + item.Value;
                }
            }

            public PackedInfo(IDictionary<int, List<IGoods>> packedGoodsList)
            {
                _packedGoodsList = new List<IGoods>();
                _packedValue = 0;
                _subPackedValue = 0;
                foreach (KeyValuePair<int, List<IGoods>> kvp in packedGoodsList)
                foreach (IGoods item in kvp.Value)
                {
                    _packedGoodsList.Add(item);
                    _packedSize = _packedSize + item.Size;
                    if (item is IGoodsGroup)
                        _packedValue = _packedValue + item.Value;
                    else
                        _subPackedValue = _subPackedValue + item.Value;
                }
            }

            private readonly List<IGoods> _packedGoodsList;

            /// <summary>
            /// 被打包的物品队列(未被拆解为最小粒度的物品)
            /// </summary>
            public IList<IGoods> PackedGoodsList
            {
                get { return _packedGoodsList; }
            }

            private int _packedSize;

            /// <summary>
            /// 被打包物品的规格
            /// </summary>
            public int PackedSize
            {
                get { return _packedSize; }
            }

            private int _packedValue;

            /// <summary>
            /// 被打包物品的价值
            /// </summary>
            public int PackedValue
            {
                get { return _packedValue; }
            }

            private int _subPackedValue;

            /// <summary>
            /// 被打包零散物品的价值
            /// </summary>
            public int SubPackedValue
            {
                get { return _subPackedValue; }
            }

            public void AddRange(PackedInfo packedInfo)
            {
                _packedGoodsList.AddRange(packedInfo._packedGoodsList);
                _packedSize = _packedSize + packedInfo._packedSize;
                _packedValue = _packedValue + packedInfo._packedValue;
                _subPackedValue = _subPackedValue + packedInfo._subPackedValue;
            }
        }

        #region 方法

        private static List<IGoods> GetLeafGoodsList(IList<IGoods> goodsList, bool shallow)
        {
            List<IGoods> result = new List<IGoods>();
            if (goodsList != null && goodsList.Count > 0)
                foreach (IGoods item in goodsList)
                {
                    if (item is IGoodsGroup goodsGroup)
                    {
                        if (goodsGroup.SubList != null)
                            result.AddRange(shallow ? goodsGroup.SubList : GetLeafGoodsList(goodsGroup.SubList, false));
                    }
                    else
                        result.Add(item);
                }

            return result;
        }

        private static void FindCharacter(IList<IGoods> goodsList, ref int zeroCount, ref int minSize, ref int toolSize)
        {
            foreach (IGoods item in goodsList)
                if (item is IGoodsGroup goodsGroup)
                    FindCharacter(goodsGroup.SubList, ref zeroCount, ref minSize, ref toolSize);
                else
                {
                    if (zeroCount > 0)
                    {
                        int z = 0;
                        int size = item.Size;
                        while (size > 0 && size % 10 == 0)
                        {
                            size = size / 10;
                            z = z + 1;
                        }

                        if (zeroCount > z)
                            zeroCount = z;
                    }

                    if (minSize > item.Size)
                        minSize = item.Size;
                    toolSize = toolSize + item.Size;
                }
        }

        /// <summary>
        /// 打包(在超载范围内需撑满)
        /// adienceSize = knapsackSize
        /// </summary>
        /// <param name="knapsackSize">背包规格</param>
        /// <param name="overloadRange">超载范围</param>
        /// <returns>是否成功</returns>
        public bool Pack(int knapsackSize, int overloadRange = 0)
        {
            return Pack(knapsackSize, overloadRange, knapsackSize + overloadRange);
        }

        /// <summary>
        /// 打包(在超载范围内需撑满)
        /// </summary>
        /// <param name="knapsackSize">背包规格</param>
        /// <param name="overloadRange">超载范围</param>
        /// <param name="approachSize">趋近规格(超出范围则就近)</param>
        /// <returns>是否成功</returns>
        public bool Pack(int knapsackSize, int overloadRange, int approachSize)
        {
            if (_goodsList == null || _goodsList.Count == 0)
                throw new InvalidOperationException("物品(/组)队列goodsList不允许为空");
            if (overloadRange < 0)
            {
                knapsackSize = knapsackSize + overloadRange;
                overloadRange = Math.Abs(overloadRange);
            }

            if (knapsackSize < 0)
            {
                overloadRange = overloadRange + knapsackSize;
                if (overloadRange <= 0)
                    throw new InvalidOperationException(String.Format("背包规格knapsackSize({0})或超载范围overloadRange({1})不允许为负", knapsackSize, overloadRange));
                knapsackSize = 0;
            }

            int maxKnapsackSize = knapsackSize + overloadRange;
            if (approachSize < knapsackSize)
                approachSize = knapsackSize;
            else if (approachSize > maxKnapsackSize)
                approachSize = maxKnapsackSize;
            PackedInfo packedInfo = DoPack(_goodsList, knapsackSize, maxKnapsackSize, approachSize, true);
            if (packedInfo != null)
            {
                _packedGoodsList = GetLeafGoodsList(packedInfo.PackedGoodsList, false);
                _packedSize = packedInfo.PackedSize;
                _packedValue = packedInfo.PackedValue;
                _subPackedValue = packedInfo.SubPackedValue;
                return true;
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static PackedInfo DoPack(IList<IGoods> goodsList, int knapsackSize, int maxKnapsackSize, int approachSize, bool tryLimit)
        {
            int zeroCount = Int32.MaxValue;
            int minSize = Int32.MaxValue;
            int toolSize = 0;
            FindCharacter(goodsList, ref zeroCount, ref minSize, ref toolSize);

            if (knapsackSize != maxKnapsackSize && knapsackSize > toolSize)
                return null;

            PackedInfo result;
            if (tryLimit)
            {
                int limitSize = LIMIT_SIZE_STEP;
                while (maxKnapsackSize - minSize > limitSize)
                {
                    if (limitSize > LimitSizeTryMaximum)
                        throw new InvalidOperationException("超出尝试运算极限范围");
                    result = DoPack(goodsList, maxKnapsackSize - minSize - limitSize);
                    if (result != null && result.PackedSize > 0)
                    {
                        List<IGoods> limitGoodsList = new List<IGoods>(goodsList.Count);
                        foreach (IGoods item in goodsList)
                            if (!result.PackedGoodsList.Contains(item))
                                limitGoodsList.Add(item);
                        PackedInfo limitPackedInfo = DoPack(limitGoodsList, knapsackSize - result.PackedSize, maxKnapsackSize - result.PackedSize, approachSize - result.PackedSize,
                            maxKnapsackSize - result.PackedSize - minSize < limitSize);
                        if (limitPackedInfo != null)
                        {
                            result.AddRange(limitPackedInfo);
                            return result;
                        }
                    }

                    limitSize = limitSize + LIMIT_SIZE_STEP;
                }
            }

            if (knapsackSize == maxKnapsackSize)
                return knapsackSize >= toolSize ? new PackedInfo(goodsList) : DoPack(goodsList, knapsackSize, maxKnapsackSize, approachSize, zeroCount, minSize);

            List<IGoods> goodsGroupList = new List<IGoods>(goodsList.Count);
            int groupToolSize = 0;
            int groupMinSize = Int32.MaxValue;
            foreach (IGoods goods in goodsList)
                if (goods is IGoodsGroup goodsGroup && !goodsGroup.Ignore)
                {
                    goodsGroupList.Add(goodsGroup);
                    groupToolSize = groupToolSize + goodsGroup.Size;
                    if (groupMinSize > goodsGroup.Size)
                        groupMinSize = goodsGroup.Size;
                }

            if (knapsackSize <= groupToolSize)
            {
                result = DoPack(goodsGroupList, knapsackSize, maxKnapsackSize, approachSize, zeroCount, groupMinSize);
                if (result != null)
                    return result;
            }

            result = null;
            List<Task<PackedInfo>> tasks = new List<Task<PackedInfo>>();
            foreach (IGoods goods in goodsList)
                if (goods is IGoodsGroup goodsGroup)
                {
                    List<IGoods> mixGoodsList = new List<IGoods>(goodsGroupList);
                    mixGoodsList.Remove(goodsGroup);
                    mixGoodsList.AddRange(goodsGroup.SubList);
                    tasks.Add(Task<PackedInfo>.Factory.StartNew(() => DoPack(mixGoodsList, knapsackSize, maxKnapsackSize, approachSize, zeroCount, minSize)));
                }

            Task.WaitAll(tasks.ToArray());
            foreach (Task<PackedInfo> task in tasks)
                if (task.Result != null)
                {
                    if (result == null ||
                        task.Result.PackedValue > result.PackedValue ||
                        task.Result.PackedValue == result.PackedValue &&
                        (task.Result.SubPackedValue > result.SubPackedValue || task.Result.SubPackedValue == result.SubPackedValue && Math.Abs(approachSize - result.PackedSize) > Math.Abs(approachSize - task.Result.PackedSize)))
                        result = task.Result;
                }

            if (result != null)
                return result;

            tasks.Clear();
            for (int i = 0; i < goodsList.Count; i++)
                if (goodsList[i] is IGoodsGroup item1)
                    for (int j = i + 1; j < goodsList.Count; j++)
                        if (goodsList[j] is IGoodsGroup item2)
                        {
                            List<IGoods> mixGoodsList = new List<IGoods>(goodsGroupList);
                            mixGoodsList.Remove(item1);
                            mixGoodsList.Remove(item2);
                            mixGoodsList.AddRange(item1.SubList);
                            mixGoodsList.AddRange(item2.SubList);
                            tasks.Add(Task<PackedInfo>.Factory.StartNew(() => DoPack(mixGoodsList, knapsackSize, maxKnapsackSize, approachSize, zeroCount, minSize)));
                        }

            Task.WaitAll(tasks.ToArray());
            foreach (Task<PackedInfo> task in tasks)
                if (task.Result != null)
                {
                    if (result == null ||
                        task.Result.PackedValue > result.PackedValue ||
                        task.Result.PackedValue == result.PackedValue &&
                        (task.Result.SubPackedValue > result.SubPackedValue || task.Result.SubPackedValue == result.SubPackedValue && Math.Abs(approachSize - result.PackedSize) > Math.Abs(approachSize - task.Result.PackedSize)))
                        result = task.Result;
                }

            return result;
        }

        private static PackedInfo DoPack(IList<IGoods> goodsList, int knapsackSize)
        {
            SortedList<int, List<IGoods>> result = new SortedList<int, List<IGoods>>();
            int packedSize = 0;
            foreach (IGoods item in goodsList)
            {
                if (item.Size > knapsackSize)
                    continue;
                if (packedSize < knapsackSize)
                {
                    List<IGoods> value;
                    if (!result.TryGetValue(item.Value, out value))
                    {
                        value = new List<IGoods>();
                        result.Add(item.Value, value);
                    }

                    value.Add(item);
                    packedSize = packedSize + item.Size;
                    continue;
                }

                KeyValuePair<int, List<IGoods>> first = result.First();
                if (item.Value > first.Key)
                {
                    packedSize = packedSize - first.Value[0].Size;
                    first.Value.RemoveAt(0);
                    if (first.Value.Count == 0)
                        result.Remove(first.Key);
                    List<IGoods> value;
                    if (!result.TryGetValue(item.Value, out value))
                    {
                        value = new List<IGoods>();
                        result.Add(item.Value, value);
                    }

                    value.Add(item);
                    packedSize = packedSize + item.Size;
                }
            }

            while (packedSize > knapsackSize)
            {
                KeyValuePair<int, List<IGoods>> first = result.First();
                IGoods item = first.Value[0];
                packedSize = packedSize - item.Size;
                first.Value.RemoveAt(0);
                if (first.Value.Count == 0)
                    result.Remove(first.Key);
            }

            return new PackedInfo(result);
        }

        private static PackedInfo DoPack(IList<IGoods> goodsList, int knapsackSize, int maxKnapsackSize, int approachSize, int zeroCount, int minSize)
        {
            int precision = (int) Math.Pow(10, zeroCount);
            int minSizeP = minSize / precision;
            int maxKnapsackSizeP = maxKnapsackSize / precision;
            int[] matrixL = new int[maxKnapsackSizeP + 1];
            int[] matrixR = new int[maxKnapsackSizeP + 1];
            SortedList<int, HashSet<int>> putinList = new SortedList<int, HashSet<int>>(goodsList.Count);
            for (int i = 0; i < goodsList.Count; i++)
            {
                HashSet<int> putin = new HashSet<int>();
                IGoods goods = goodsList[i];
                int goodsSizeP = goods.Size / precision;
                for (int s = minSizeP; s <= maxKnapsackSizeP; s++)
                {
                    int marginSizeP = s - goodsSizeP; //s规格的背包放入goods后的余量是marginSize
                    if (marginSizeP >= 0) //s规格的背包放得下goods
                    {
                        int value = matrixL[marginSizeP] + goods.Value; //与前轮（第0...i-1件）规划得到的marginSize规格的背包合拼
                        if (value > matrixL[s]) //放下后的价值更高
                        {
                            matrixR[s] = value;
                            putin.Add(s);
                            continue;
                        }
                    }

                    matrixR[s] = matrixL[s];
                }

                int[] matrixT = matrixL;
                matrixL = matrixR;
                matrixR = matrixT;
                putinList[i] = putin;
            }

            if (knapsackSize == maxKnapsackSize)
            {
                List<IGoods> packedGoodsList = new List<IGoods>();
                int canPackedSizeP = maxKnapsackSizeP;
                for (int i = goodsList.Count - 1; i >= 0; i--)
                {
                    if (putinList[i].Contains(canPackedSizeP))
                    {
                        IGoods goods = goodsList[i];
                        packedGoodsList.Add(goods);
                        canPackedSizeP = canPackedSizeP - goods.Size / precision;
                        if (canPackedSizeP < minSizeP)
                            break;
                    }
                }

                return packedGoodsList.Count > 0 ? new PackedInfo(packedGoodsList) : null;
            }

            IList<IGoods> selectedGoodsList = null;
            int selectedSize = 0;
            for (int i1 = goodsList.Count - 1; i1 >= 0; i1--)
            {
                IList<IGoods> packedGoodsList = new List<IGoods>();
                int packedSize = 0;
                int canPackedSizeP = maxKnapsackSizeP;
                for (int i2 = 0; i2 <= goodsList.Count - 1; i2++)
                {
                    int i = i1 - i2;
                    if (i < 0)
                        i = i + goodsList.Count;
                    if (putinList[i].Contains(canPackedSizeP))
                    {
                        IGoods goods = goodsList[i];
                        packedGoodsList.Add(goods);
                        packedSize = packedSize + goods.Size;
                        canPackedSizeP = canPackedSizeP - goods.Size / precision;
                        if (canPackedSizeP < minSizeP)
                            break;
                    }
                }

                if (packedSize >= knapsackSize)
                {
                    if (packedSize > approachSize)
                        DoApproach(ref packedGoodsList, ref packedSize, knapsackSize, approachSize);
                    if (selectedGoodsList == null || Math.Abs(approachSize - selectedSize) > Math.Abs(approachSize - packedSize))
                    {
                        selectedGoodsList = packedGoodsList;
                        selectedSize = packedSize;
                        if (Math.Abs(approachSize - selectedSize) < minSize)
                            return new PackedInfo(selectedGoodsList);
                    }
                }
            }

            return selectedGoodsList != null ? new PackedInfo(selectedGoodsList) : null;
        }

        private static void DoApproach(ref IList<IGoods> packedGoodsList, ref int packedSize, int knapsackSize, int approachSize)
        {
            SortedList<int, List<IGoods>> sortedList = new SortedList<int, List<IGoods>>();
            foreach (IGoods item in packedGoodsList)
            {
                List<IGoods> goodsList;
                if (!sortedList.TryGetValue(item.Value, out goodsList))
                {
                    goodsList = new List<IGoods>();
                    sortedList.Add(item.Value, goodsList);
                }

                goodsList.Add(item);
            }

            do
            {
                List<IGoods> firstGoodsList = sortedList.First().Value;
                IGoods selectedGoods = null;
                foreach (IGoods item in firstGoodsList)
                    if (packedSize - item.Size >= knapsackSize)
                        if (selectedGoods == null || Math.Abs(approachSize - (packedSize - selectedGoods.Size)) > Math.Abs(approachSize - (packedSize - item.Size)))
                            selectedGoods = item;
                if (selectedGoods != null)
                {
                    firstGoodsList.Remove(selectedGoods);
                    if (firstGoodsList.Count == 0)
                        sortedList.Remove(selectedGoods.Value);
                    packedGoodsList.Remove(selectedGoods);
                    packedSize = packedSize - selectedGoods.Size;
                }
                else
                    break;
            } while (true);
        }

        #endregion
    }
}
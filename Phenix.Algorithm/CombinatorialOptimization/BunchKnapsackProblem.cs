using System;
using System.Collections.Generic;

namespace Phenix.Algorithm.CombinatorialOptimization
{
    /// <summary>
    /// 集束背包问题的动态规划+贪心算法
    /// 从M支集束的N个物品中挑选出一个尽可能整支和价值高的子集使其装满容量为W且允许有一定超载范围的背包
    /// </summary>
    public static class BunchKnapsackProblem
    {
        #region 属性

        private const int ARRAY_MAX_SIZE = 10000;

        #endregion

        #region 方法

        /// <summary>
        /// 打包(在超载范围内需撑满)
        /// </summary>
        /// <param name="goodsList">一组物品(含集束)</param>
        /// <param name="knapsackSize">背包规格</param>
        /// <param name="overloadRange">超载范围</param>
        /// <returns>是否成功</returns>
        public static PackedBunches Pack(IList<IGoods> goodsList, int knapsackSize, int overloadRange)
        {
            if (goodsList == null)
                throw new ArgumentNullException(nameof(goodsList));
            if (overloadRange < 0)
                throw new ArgumentOutOfRangeException(nameof(overloadRange));

            PackedBunches result = DoPack(goodsList, knapsackSize, overloadRange);
            return result != null && result.Size >= knapsackSize ? result : null;
        }

        private static PackedBunches DoPack(IList<IGoods> goodsList, int knapsackSize, int overloadRange)
        {
            PackedBunches packedBunches = PassablyPack(goodsList, knapsackSize + overloadRange, knapsackSize);
            if (packedBunches != null && packedBunches.Size >= knapsackSize)
                return packedBunches;
            return DoPack(goodsList, knapsackSize, overloadRange, packedBunches);
        }

        private static PackedBunches DoPack(IList<IGoods> goodsList, int knapsackSize, int overloadRange, PackedBunches packedBunches)
        {
            if (packedBunches != null)
                knapsackSize = knapsackSize - packedBunches.Size;

            List<IGoods> children = new List<IGoods>();
            foreach (IGoodsBunch item in goodsList)
                if (item != null && item.Items != null && item.Size > 0 && (packedBunches == null || !packedBunches.Value.Contains(item)))
                    children.AddRange(item.Items);
            PackedBunches result = DoPack(children, knapsackSize, overloadRange);
            if (result != null && result.Size >= knapsackSize)
            {
                result.AddRange(packedBunches);
                return result;
            }

            return packedBunches != null ? DoPack(goodsList, knapsackSize + packedBunches.Size, overloadRange, null) : null;
        }

        private static PackedBunches PassablyPack(IList<IGoods> goodsList, int knapsackSize, int minPackSize)
        {
            int zeroCount = Int32.MaxValue;
            int minGoodsSize = Int32.MaxValue;
            int toolSize = 0;
            foreach (IGoods item in goodsList)
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

                if (minGoodsSize > item.Size)
                    minGoodsSize = item.Size;
                toolSize = toolSize + item.Size;
            }

            if (knapsackSize >= toolSize)
                return minPackSize <= toolSize ? new PackedBunches(toolSize, goodsList) : null;

            if (minPackSize < minGoodsSize)
                minPackSize = minGoodsSize;

            int precision = (int) Math.Pow(10, zeroCount);
            int knapsackSizeP = knapsackSize / precision;
            if (knapsackSizeP >= ARRAY_MAX_SIZE)
                return PassablyPackBig(goodsList, knapsackSize, minPackSize);
            int minPackSizeP = minPackSize / precision;
            int minSizeP = minGoodsSize / precision;
            int[] matrixL = new int[knapsackSizeP + 1];
            int[] matrixR = new int[knapsackSizeP + 1];
            Dictionary<int, SortedSet<int>> putinSizeDictionary = new Dictionary<int, SortedSet<int>>(goodsList.Count);
            for (int i = 0; i < goodsList.Count; i++)
            {
                IGoods goods = goodsList[i];
                SortedSet<int> putinSizeList = new SortedSet<int>();
                int goodsSizeP = goods.Size / precision;
                for (int s = minSizeP; s <= knapsackSizeP; s++)
                {
                    int marginSizeP = s - goodsSizeP; //s规格的背包放入goods后的余量是marginSize
                    if (marginSizeP >= 0) //s规格的背包放得下goods
                    {
                        int value = matrixL[marginSizeP] + goods.Value; //与前轮（第0...i-1件）规划得到的marginSize规格的背包合拼
                        if (value > matrixL[s]) //放下后的价值更高
                        {
                            matrixR[s] = value;
                            putinSizeList.Add(s);
                            continue;
                        }
                    }

                    matrixR[s] = matrixL[s];
                }

                int[] matrixT = matrixL;
                matrixL = matrixR;
                matrixR = matrixT;
                putinSizeDictionary.Add(i, putinSizeList);
            }

            List<IGoods> result = new List<IGoods>();
            int canPackSizeP;
            for (int ii = goodsList.Count - 1; ii >= 0; ii--)
            {
                SortedSet<int> putinSizeList = putinSizeDictionary[ii];
                if (putinSizeList.Count == 0)
                    continue;
                for (int s = knapsackSizeP; s >= minPackSizeP; s--)
                    if (putinSizeList.Contains(s))
                    {
                        result.Clear();
                        IGoods goods = goodsList[ii];
                        result.Add(goods);
                        canPackSizeP = s - goods.Size / precision;
                        for (int i = ii - 1; i >= 0; i--)
                            if (putinSizeDictionary[i].Contains(canPackSizeP))
                            {
                                goods = goodsList[i];
                                result.Add(goods);
                                canPackSizeP = canPackSizeP - goods.Size / precision;
                                if (canPackSizeP < minSizeP)
                                    break;
                            }

                        if (s - canPackSizeP >= minPackSizeP)
                            return new PackedBunches((s - canPackSizeP) * precision, result);
                    }
            }

            result.Clear();
            canPackSizeP = knapsackSizeP;
            for (int i = goodsList.Count - 1; i >= 0; i--)
                if (putinSizeDictionary[i].Contains(canPackSizeP))
                {
                    IGoods goods = goodsList[i];
                    result.Add(goods);
                    canPackSizeP = canPackSizeP - goods.Size / precision;
                    if (canPackSizeP < minSizeP)
                        break;
                }

            return new PackedBunches((knapsackSizeP - canPackSizeP) * precision, result);
        }

        private static PackedBunches PassablyPackBig(IList<IGoods> goodsList, int knapsackSize, int minPackSize)
        {
            int zeroCount = Int32.MaxValue;
            int minGoodsSize = Int32.MaxValue;
            int toolSize = 0;
            foreach (IGoods item in goodsList)
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

                if (minGoodsSize > item.Size)
                    minGoodsSize = item.Size;
                toolSize = toolSize + item.Size;
            }

            if (knapsackSize >= toolSize)
                return minPackSize <= toolSize ? new PackedBunches(toolSize, goodsList) : null;

            if (minPackSize < minGoodsSize)
                minPackSize = minGoodsSize;

            int precision = (int)Math.Pow(10, zeroCount);
            int knapsackSizeP = knapsackSize / precision;
            int minPackSizeP = minPackSize / precision;
            int minSizeP = minGoodsSize / precision;
            int matrixCount = knapsackSizeP % ARRAY_MAX_SIZE > 0 ? knapsackSizeP / ARRAY_MAX_SIZE + 1 : knapsackSizeP / ARRAY_MAX_SIZE;
            List<int[]> matrixL = new List<int[]>(matrixCount);
            for (int i = 1; i <= matrixCount; i++)
                matrixL.Add(i == matrixCount ? new int[knapsackSizeP % ARRAY_MAX_SIZE + 1] : new int[ARRAY_MAX_SIZE]);
            List<int[]> matrixR = new List<int[]>(matrixCount);
            for (int i = 1; i <= matrixCount; i++)
                matrixR.Add(i == matrixCount ? new int[knapsackSizeP % ARRAY_MAX_SIZE + 1] : new int[ARRAY_MAX_SIZE]);
            Dictionary<int, SortedSet<int>> putinSizeDictionary = new Dictionary<int, SortedSet<int>>(goodsList.Count);
            for (int i = 0; i < goodsList.Count; i++)
            {
                IGoods goods = goodsList[i];
                SortedSet<int> putinSizeList = new SortedSet<int>();
                int goodsSizeP = goods.Size / precision;
                for (int s = minSizeP; s <= knapsackSizeP; s++)
                {
                    int s1 = s % ARRAY_MAX_SIZE;
                    int s2 = s / ARRAY_MAX_SIZE;
                    int marginSizeP = s - goodsSizeP; //s规格的背包放入goods后的余量是marginSize
                    if (marginSizeP >= 0) //s规格的背包放得下goods
                    {
                        int value = matrixL[marginSizeP / ARRAY_MAX_SIZE][marginSizeP % ARRAY_MAX_SIZE] + goods.Value; //与前轮（第0...i-1件）规划得到的marginSize规格的背包合拼
                        if (value > matrixL[s2][s1]) //放下后的价值更高
                        {
                            matrixR[s2][s1] = value;
                            putinSizeList.Add(s);
                            continue;
                        }
                    }

                    matrixR[s2][s1] = matrixL[s2][s1];
                }

                List<int[]> matrixT = matrixL;
                matrixL = matrixR;
                matrixR = matrixT;
                putinSizeDictionary.Add(i, putinSizeList);
            }

            List<IGoods> result = new List<IGoods>();
            int canPackSizeP;
            for (int ii = goodsList.Count - 1; ii >= 0; ii--)
            {
                SortedSet<int> putinSizeList = putinSizeDictionary[ii];
                if (putinSizeList.Count == 0)
                    continue;
                for (int s = knapsackSizeP; s >= minPackSizeP; s--)
                    if (putinSizeList.Contains(s))
                    {
                        result.Clear();
                        IGoods goods = goodsList[ii];
                        result.Add(goods);
                        canPackSizeP = s - goods.Size / precision;
                        for (int i = ii - 1; i >= 0; i--)
                            if (putinSizeDictionary[i].Contains(canPackSizeP))
                            {
                                goods = goodsList[i];
                                result.Add(goods);
                                canPackSizeP = canPackSizeP - goods.Size / precision;
                                if (canPackSizeP < minSizeP)
                                    break;
                            }

                        if (s - canPackSizeP >= minPackSizeP)
                            return new PackedBunches((s - canPackSizeP) * precision, result);
                    }
            }

            result.Clear();
            canPackSizeP = knapsackSizeP;
            for (int i = goodsList.Count - 1; i >= 0; i--)
                if (putinSizeDictionary[i].Contains(canPackSizeP))
                {
                    IGoods goods = goodsList[i];
                    result.Add(goods);
                    canPackSizeP = canPackSizeP - goods.Size / precision;
                    if (canPackSizeP < minSizeP)
                        break;
                }

            return new PackedBunches((knapsackSizeP - canPackSizeP) * precision, result);
        }

        #endregion
    }
}
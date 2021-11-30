using System;
using System.Collections.Generic;

namespace Phenix.Algorithm.CombinatorialOptimization
{
    /// <summary>
    /// 0-1背包问题的动态规划算法
    /// 从N个物品中挑选出一个价值最高的子集使其尽可能装满容量为W的背包
    /// </summary>
    public static class ZeroOneKnapsack
    {
        #region 属性

        private const int ArrayMaxSize = 10000;

        #endregion

        #region 方法

        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="goodsList">一组物品</param>
        /// <param name="knapsackSize">背包规格</param>
        /// <returns>挑选出的子集</returns>
        public static IList<IGoods> Pack(IList<IGoods> goodsList, int knapsackSize)
        {
            if (goodsList == null)
                throw new ArgumentNullException(nameof(goodsList));

            if (knapsackSize <= 0)
                return null;

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
                return goodsList;

            int precision = (int) Math.Pow(10, zeroCount);
            int knapsackSizeP = knapsackSize / precision;
            if (knapsackSizeP >= ArrayMaxSize)
                return PackBig(goodsList, knapsackSize);
            int minSizeP = minGoodsSize / precision;
            int[] matrixL = new int[knapsackSizeP + 1];
            int[] matrixR = new int[knapsackSizeP + 1];
            Stack<SortedSet<int>> putinSizeStack = new Stack<SortedSet<int>>(goodsList.Count);
            foreach (IGoods goods in goodsList)
            {
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
                putinSizeStack.Push(putinSizeList);
            }

            List<IGoods> result = new List<IGoods>();
            int canPackSizeP = knapsackSizeP;
            while (putinSizeStack.Count > 0)
            {
                if (!putinSizeStack.Pop().Contains(canPackSizeP))
                    continue;
                IGoods goods = goodsList[putinSizeStack.Count];
                result.Add(goods);
                canPackSizeP = canPackSizeP - goods.Size / precision;
                if (canPackSizeP < minSizeP)
                    break;
            }

            return result;
        }

        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="goodsList">一组物品</param>
        /// <param name="knapsackSize">背包规格</param>
        /// <returns>挑选出的子集</returns>
        public static IList<IGoods> PackBig(IList<IGoods> goodsList, int knapsackSize)
        {
            if (goodsList == null)
                throw new ArgumentNullException(nameof(goodsList));

            if (knapsackSize <= 0)
                return null;

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
                return goodsList;

            int precision = (int)Math.Pow(10, zeroCount);
            int knapsackSizeP = knapsackSize / precision;
            int minSizeP = minGoodsSize / precision;
            int matrixCount = knapsackSizeP % ArrayMaxSize > 0 ? knapsackSizeP / ArrayMaxSize + 1 : knapsackSizeP / ArrayMaxSize;
            List<int[]> matrixL = new List<int[]>(matrixCount);
            for (int i = 1; i <= matrixCount; i++)
                matrixL.Add(i == matrixCount ? new int[knapsackSizeP % ArrayMaxSize + 1] : new int[ArrayMaxSize]);
            List<int[]> matrixR = new List<int[]>(matrixCount);
            for (int i = 1; i <= matrixCount; i++)
                matrixR.Add(i == matrixCount ? new int[knapsackSizeP % ArrayMaxSize + 1] : new int[ArrayMaxSize]);
            Stack<SortedSet<int>> putinSizeStack = new Stack<SortedSet<int>>(goodsList.Count);
            foreach (IGoods goods in goodsList)
            {
                SortedSet<int> putinSizeList = new SortedSet<int>();
                int goodsSizeP = goods.Size / precision;
                for (int s = minSizeP; s <= knapsackSizeP; s++)
                {
                    int s1 = s % ArrayMaxSize;
                    int s2 = s / ArrayMaxSize;
                    int marginSizeP = s - goodsSizeP; //s规格的背包放入goods后的余量是marginSize
                    if (marginSizeP >= 0) //s规格的背包放得下goods
                    {
                        int value = matrixL[marginSizeP / ArrayMaxSize][marginSizeP % ArrayMaxSize] + goods.Value; //与前轮（第0...i-1件）规划得到的marginSize规格的背包合拼
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
                putinSizeStack.Push(putinSizeList);
            }

            List<IGoods> result = new List<IGoods>();
            int canPackSizeP = knapsackSizeP;
            while (putinSizeStack.Count > 0)
            {
                if (!putinSizeStack.Pop().Contains(canPackSizeP))
                    continue;
                IGoods goods = goodsList[putinSizeStack.Count];
                result.Add(goods);
                canPackSizeP = canPackSizeP - goods.Size / precision;
                if (canPackSizeP < minSizeP)
                    break;
            }

            return result;
        }

        /// <summary>
        /// 打包至少占有一定容量
        /// </summary>
        /// <param name="goodsList">一组物品</param>
        /// <param name="knapsackSize">背包规格</param>
        /// <param name="minPackSize">最小打包规格</param>
        /// <param name="aimForMinSize">趋向最小规格且不考虑打包价值</param>
        /// <returns>挑选出的子集</returns>
        public static IList<IGoods> Pack(IList<IGoods> goodsList, int knapsackSize, int minPackSize, bool aimForMinSize = false)
        {
            if (goodsList == null)
                throw new ArgumentNullException(nameof(goodsList));
            if (minPackSize > knapsackSize)
                throw new ArgumentOutOfRangeException(nameof(minPackSize));

            if (knapsackSize <= 0)
                return null;

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
                return minPackSize <= toolSize ? goodsList : null;

            if (minPackSize < minGoodsSize)
                minPackSize = minGoodsSize;

            int precision = (int) Math.Pow(10, zeroCount);
            int knapsackSizeP = knapsackSize / precision;
            if (knapsackSizeP >= ArrayMaxSize)
                return PackBig(goodsList, knapsackSize, minPackSize, aimForMinSize);
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
            if (!aimForMinSize)
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
                            int canPackSizeP = s - goods.Size / precision;
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
                                return result;
                        }
                }
            else
                for (int ii = goodsList.Count - 1; ii >= 0; ii--)
                {
                    SortedSet<int> putinSizeList = putinSizeDictionary[ii];
                    if (putinSizeList.Count == 0)
                        continue;
                    for (int s = minPackSizeP; s <= knapsackSizeP; s++)
                        if (putinSizeList.Contains(s))
                        {
                            result.Clear();
                            IGoods goods = goodsList[ii];
                            result.Add(goods);
                            int canPackSizeP = s - goods.Size / precision;
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
                                return result;
                        }
                }

            return null;
        }

        /// <summary>
        /// 打包至少占有一定容量
        /// </summary>
        /// <param name="goodsList">一组物品</param>
        /// <param name="knapsackSize">背包规格</param>
        /// <param name="minPackSize">最小打包规格</param>
        /// <param name="aimForMinSize">趋向最小规格且不考虑打包价值</param>
        /// <returns>挑选出的子集</returns>
        public static IList<IGoods> PackBig(IList<IGoods> goodsList, int knapsackSize, int minPackSize, bool aimForMinSize = false)
        {
            if (goodsList == null)
                throw new ArgumentNullException(nameof(goodsList));
            if (minPackSize > knapsackSize)
                throw new ArgumentOutOfRangeException(nameof(minPackSize));

            if (knapsackSize <= 0)
                return null;

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
                return minPackSize <= toolSize ? goodsList : null;

            if (minPackSize < minGoodsSize)
                minPackSize = minGoodsSize;

            int precision = (int)Math.Pow(10, zeroCount);
            int knapsackSizeP = knapsackSize / precision;
            int minPackSizeP = minPackSize / precision;
            int minSizeP = minGoodsSize / precision;
            int matrixCount = knapsackSizeP % ArrayMaxSize > 0 ? knapsackSizeP / ArrayMaxSize + 1 : knapsackSizeP / ArrayMaxSize;
            List<int[]> matrixL = new List<int[]>(matrixCount);
            for (int i = 1; i <= matrixCount; i++)
                matrixL.Add(i == matrixCount ? new int[knapsackSizeP % ArrayMaxSize + 1] : new int[ArrayMaxSize]);
            List<int[]> matrixR = new List<int[]>(matrixCount);
            for (int i = 1; i <= matrixCount; i++)
                matrixR.Add(i == matrixCount ? new int[knapsackSizeP % ArrayMaxSize + 1] : new int[ArrayMaxSize]);
            Dictionary<int, SortedSet<int>> putinSizeDictionary = new Dictionary<int, SortedSet<int>>(goodsList.Count);
            for (int i = 0; i < goodsList.Count; i++)
            {
                IGoods goods = goodsList[i];
                SortedSet<int> putinSizeList = new SortedSet<int>();
                int goodsSizeP = goods.Size / precision;
                for (int s = minSizeP; s <= knapsackSizeP; s++)
                {
                    int s1 = s % ArrayMaxSize;
                    int s2 = s / ArrayMaxSize;
                    int marginSizeP = s - goodsSizeP; //s规格的背包放入goods后的余量是marginSize
                    if (marginSizeP >= 0) //s规格的背包放得下goods
                    {
                        int value = matrixL[marginSizeP / ArrayMaxSize][marginSizeP % ArrayMaxSize] + goods.Value; //与前轮（第0...i-1件）规划得到的marginSize规格的背包合拼
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
            if (!aimForMinSize)
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
                            int canPackSizeP = s - goods.Size / precision;
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
                                return result;
                        }
                }
            else
                for (int ii = goodsList.Count - 1; ii >= 0; ii--)
                {
                    SortedSet<int> putinSizeList = putinSizeDictionary[ii];
                    if (putinSizeList.Count == 0)
                        continue;
                    for (int s = minPackSizeP; s <= knapsackSizeP; s++)
                        if (putinSizeList.Contains(s))
                        {
                            result.Clear();
                            IGoods goods = goodsList[ii];
                            result.Add(goods);
                            int canPackSizeP = s - goods.Size / precision;
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
                                return result;
                        }
                }

            return null;
        }

        /// <summary>
        /// 打包至少占有一定容量且不低于一定价值
        /// </summary>
        /// <param name="goodsList">一组物品</param>
        /// <param name="knapsackSize">背包规格</param>
        /// <param name="minPackSize">最小打包规格</param>
        /// <param name="minPackValue">最低打包价值</param>
        /// <param name="aimForMinSize">趋向最小规格且只要满足最低打包价值</param>
        /// <returns>序号-挑选出的子集</returns>
        public static IDictionary<int, IList<IGoods>> Pack(IList<IGoods> goodsList, int knapsackSize, int minPackSize, int minPackValue, bool aimForMinSize = false)
        {
            if (goodsList == null)
                throw new ArgumentNullException(nameof(goodsList));
            if (minPackSize > knapsackSize)
                throw new ArgumentOutOfRangeException(nameof(minPackSize));

            if (knapsackSize <= 0)
                return null;

            int zeroCount = Int32.MaxValue;
            int minGoodsSize = Int32.MaxValue;
            int toolSize = 0;
            int toolValue = 0;
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
                toolValue = toolValue + item.Value;
            }

            Dictionary<int, IList<IGoods>> result = new Dictionary<int, IList<IGoods>>();
            if (minPackValue > toolValue)
                return result;
            if (knapsackSize >= toolSize)
            {
                if (minPackSize <= toolSize)
                    result.Add(goodsList.Count - 1, goodsList);
                return result;
            }

            if (minPackSize < minGoodsSize)
                minPackSize = minGoodsSize;

            int precision = (int) Math.Pow(10, zeroCount);
            int knapsackSizeP = knapsackSize / precision;
            if (knapsackSizeP >= ArrayMaxSize)
                return PackBig(goodsList, knapsackSize, minPackSize, minPackValue, aimForMinSize);
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

            if (!aimForMinSize)
                for (int ii = goodsList.Count - 1; ii >= 0; ii--)
                {
                    SortedSet<int> putinSizeList = putinSizeDictionary[ii];
                    if (putinSizeList.Count == 0)
                        continue;
                    for (int s = knapsackSizeP; s >= minPackSizeP; s--)
                        if (putinSizeList.Contains(s))
                        {
                            int packedValue = 0;
                            List<IGoods> packedList = new List<IGoods>();
                            IGoods goods = goodsList[ii];
                            packedValue = packedValue + goods.Value;
                            packedList.Add(goods);
                            int canPackSizeP = s - goods.Size / precision;
                            for (int i = ii - 1; i >= 0; i--)
                                if (putinSizeDictionary[i].Contains(canPackSizeP))
                                {
                                    goods = goodsList[i];
                                    packedValue = packedValue + goods.Value;
                                    packedList.Add(goods);
                                    canPackSizeP = canPackSizeP - goods.Size / precision;
                                    if (canPackSizeP < minSizeP)
                                        break;
                                }

                            if (minPackValue > packedValue)
                                break;
                            if (s - canPackSizeP >= minPackSizeP)
                            {
                                result.Add(ii, packedList);
                                break;
                            }
                        }
                }
            else
                for (int ii = goodsList.Count - 1; ii >= 0; ii--)
                {
                    int packedValue = Int32.MaxValue;
                    SortedSet<int> putinSizeList = putinSizeDictionary[ii];
                    if (putinSizeList.Count == 0)
                        continue;
                    for (int s = minPackSizeP; s <= knapsackSizeP; s++)
                        if (putinSizeList.Contains(s))
                        {
                            packedValue = 0;
                            List<IGoods> packedList = new List<IGoods>();
                            IGoods goods = goodsList[ii];
                            packedValue = packedValue + goods.Value;
                            packedList.Add(goods);
                            int canPackSizeP = s - goods.Size / precision;
                            for (int i = ii - 1; i >= 0; i--)
                                if (putinSizeDictionary[i].Contains(canPackSizeP))
                                {
                                    goods = goodsList[i];
                                    packedValue = packedValue + goods.Value;
                                    packedList.Add(goods);
                                    canPackSizeP = canPackSizeP - goods.Size / precision;
                                    if (canPackSizeP < minSizeP)
                                        break;
                                }

                            if (s - canPackSizeP >= minPackSizeP && minPackValue <= packedValue)
                            {
                                result.Add(ii, packedList);
                                break;
                            }
                        }

                    if (minPackValue > packedValue)
                        break;
                }

            return result;
        }

        /// <summary>
        /// 打包至少占有一定容量且不低于一定价值
        /// </summary>
        /// <param name="goodsList">一组物品</param>
        /// <param name="knapsackSize">背包规格</param>
        /// <param name="minPackSize">最小打包规格</param>
        /// <param name="minPackValue">最低打包价值</param>
        /// <param name="aimForMinSize">趋向最小规格且只要满足最低打包价值</param>
        /// <returns>序号-挑选出的子集</returns>
        public static IDictionary<int, IList<IGoods>> PackBig(IList<IGoods> goodsList, int knapsackSize, int minPackSize, int minPackValue, bool aimForMinSize = false)
        {
            if (goodsList == null)
                throw new ArgumentNullException(nameof(goodsList));
            if (minPackSize > knapsackSize)
                throw new ArgumentOutOfRangeException(nameof(minPackSize));

            if (knapsackSize <= 0)
                return null;

            int zeroCount = Int32.MaxValue;
            int minGoodsSize = Int32.MaxValue;
            int toolSize = 0;
            int toolValue = 0;
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
                toolValue = toolValue + item.Value;
            }

            Dictionary<int, IList<IGoods>> result = new Dictionary<int, IList<IGoods>>();
            if (minPackValue > toolValue)
                return result;
            if (knapsackSize >= toolSize)
            {
                if (minPackSize <= toolSize)
                    result.Add(goodsList.Count - 1, goodsList);
                return result;
            }

            if (minPackSize < minGoodsSize)
                minPackSize = minGoodsSize;

            int precision = (int)Math.Pow(10, zeroCount);
            int knapsackSizeP = knapsackSize / precision;
            int minPackSizeP = minPackSize / precision;
            int minSizeP = minGoodsSize / precision;
            int matrixCount = knapsackSizeP % ArrayMaxSize > 0 ? knapsackSizeP / ArrayMaxSize + 1 : knapsackSizeP / ArrayMaxSize;
            List<int[]> matrixL = new List<int[]>(matrixCount);
            for (int i = 1; i <= matrixCount; i++)
                matrixL.Add(i == matrixCount ? new int[knapsackSizeP % ArrayMaxSize + 1] : new int[ArrayMaxSize]);
            List<int[]> matrixR = new List<int[]>(matrixCount);
            for (int i = 1; i <= matrixCount; i++)
                matrixR.Add(i == matrixCount ? new int[knapsackSizeP % ArrayMaxSize + 1] : new int[ArrayMaxSize]);
            Dictionary<int, SortedSet<int>> putinSizeDictionary = new Dictionary<int, SortedSet<int>>(goodsList.Count);
            for (int i = 0; i < goodsList.Count; i++)
            {
                IGoods goods = goodsList[i];
                SortedSet<int> putinSizeList = new SortedSet<int>();
                int goodsSizeP = goods.Size / precision;
                for (int s = minSizeP; s <= knapsackSizeP; s++)
                {
                    int s1 = s % ArrayMaxSize;
                    int s2 = s / ArrayMaxSize;
                    int marginSizeP = s - goodsSizeP; //s规格的背包放入goods后的余量是marginSize
                    if (marginSizeP >= 0) //s规格的背包放得下goods
                    {
                        int value = matrixL[marginSizeP / ArrayMaxSize][marginSizeP % ArrayMaxSize] + goods.Value; //与前轮（第0...i-1件）规划得到的marginSize规格的背包合拼
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

            if (!aimForMinSize)
                for (int ii = goodsList.Count - 1; ii >= 0; ii--)
                {
                    SortedSet<int> putinSizeList = putinSizeDictionary[ii];
                    if (putinSizeList.Count == 0)
                        continue;
                    for (int s = knapsackSizeP; s >= minPackSizeP; s--)
                        if (putinSizeList.Contains(s))
                        {
                            int packedValue = 0;
                            List<IGoods> packedList = new List<IGoods>();
                            IGoods goods = goodsList[ii];
                            packedValue = packedValue + goods.Value;
                            packedList.Add(goods);
                            int canPackSizeP = s - goods.Size / precision;
                            for (int i = ii - 1; i >= 0; i--)
                                if (putinSizeDictionary[i].Contains(canPackSizeP))
                                {
                                    goods = goodsList[i];
                                    packedValue = packedValue + goods.Value;
                                    packedList.Add(goods);
                                    canPackSizeP = canPackSizeP - goods.Size / precision;
                                    if (canPackSizeP < minSizeP)
                                        break;
                                }

                            if (minPackValue > packedValue)
                                break;
                            if (s - canPackSizeP >= minPackSizeP)
                            {
                                result.Add(ii, packedList);
                                break;
                            }
                        }
                }
            else
                for (int ii = goodsList.Count - 1; ii >= 0; ii--)
                {
                    int packedValue = Int32.MaxValue;
                    SortedSet<int> putinSizeList = putinSizeDictionary[ii];
                    if (putinSizeList.Count == 0)
                        continue;
                    for (int s = minPackSizeP; s <= knapsackSizeP; s++)
                        if (putinSizeList.Contains(s))
                        {
                            packedValue = 0;
                            List<IGoods> packedList = new List<IGoods>();
                            IGoods goods = goodsList[ii];
                            packedValue = packedValue + goods.Value;
                            packedList.Add(goods);
                            int canPackSizeP = s - goods.Size / precision;
                            for (int i = ii - 1; i >= 0; i--)
                                if (putinSizeDictionary[i].Contains(canPackSizeP))
                                {
                                    goods = goodsList[i];
                                    packedValue = packedValue + goods.Value;
                                    packedList.Add(goods);
                                    canPackSizeP = canPackSizeP - goods.Size / precision;
                                    if (canPackSizeP < minSizeP)
                                        break;
                                }

                            if (s - canPackSizeP >= minPackSizeP && minPackValue <= packedValue)
                            {
                                result.Add(ii, packedList);
                                break;
                            }
                        }

                    if (minPackValue > packedValue)
                        break;
                }

            return result;
        }

        #endregion
    }
}
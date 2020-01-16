using System;
using System.Collections.Generic;
using Phenix.Algorithm.CombinatorialOptimization;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("**** 演示 Phenix.Algorithm.CombinatorialOptimization 功能 ****");
            Console.WriteLine();

            Console.WriteLine("提供一组物品：");
            int[] sizes = new int[] {2, 3, 4, 5, 9}; //物品规格
            int[] values = new int[] {3, 4, 5, 8, 10}; //物品价值
            List<IGoods> goodsList = new List<IGoods>();
            for (int i = 0; i < sizes.Length; i++)
                goodsList.Add(new Goods(i, sizes[i], values[i]));
            foreach (Goods item in goodsList)
                Console.WriteLine("Index:{0}, Size={1}, Value={2}", item.Index, item.Weight, item.Value);
            Console.WriteLine();

            Console.WriteLine("挑选出打包价值最大化的可装入打包规格为{0}的背包的子集:", 20);
            foreach (Goods item in ZeroOneKnapsackProblem.Pack(goodsList, 20))
                Console.WriteLine("Index:{0}, Size={1}, Value={2}", item.Index, item.Weight, item.Value);
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("挑选出打包价值最大化的可装入打包规格为{0}—{1}的背包的子集:", 15, 10);
            IList<IGoods> packedList = ZeroOneKnapsackProblem.Pack(goodsList, 15, 10);
            if (packedList != null)
                foreach (Goods item in packedList)
                    Console.WriteLine("Index:{0}, Size={1}, Value={2}", item.Index, item.Weight, item.Value);
            else
                Console.WriteLine("无解");
            Console.WriteLine("挑选出趋向最小规格且忽略打包价值最大化的可装入打包规格为{0}—{1}的背包的子集:", 15, 10);
            packedList = ZeroOneKnapsackProblem.Pack(goodsList, 15, 10, true);
            if (packedList != null)
                foreach (Goods item in packedList)
                    Console.WriteLine("Index:{0}, Size={1}, Value={2}", item.Index, item.Weight, item.Value);
            else
                Console.WriteLine("无解");
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("挑选出打包价值最大化的可装入打包规格为{0}—{1}的背包且打包价值不低于{2}的子集:", 15, 10, 7);
            IDictionary<int, IList<IGoods>> packedDictionary = ZeroOneKnapsackProblem.Pack(goodsList, 15, 10, 7);
            if (packedDictionary != null && packedDictionary.Count > 0)
                foreach (KeyValuePair<int, IList<IGoods>> kvp in packedDictionary)
                {
                    Console.WriteLine("Pack Index:{0}", kvp.Key);
                    foreach (Goods item in kvp.Value)
                        Console.WriteLine("Index:{0}, Size={1}, Value={2}", item.Index, item.Weight, item.Value);
                    Console.WriteLine();
                }
            else
                Console.WriteLine("无解");
            Console.WriteLine("挑选出趋向最小规格且只要满足最低打包价值{2}的可装入打包规格为{0}—{1}的背包的子集:", 15, 10, 7);
            packedDictionary = ZeroOneKnapsackProblem.Pack(goodsList, 15, 10, 7, true);
            if (packedDictionary != null && packedDictionary.Count > 0)
                foreach (KeyValuePair<int, IList<IGoods>> kvp in packedDictionary)
                {
                    Console.WriteLine("Pack Index:{0}", kvp.Key);
                    foreach (Goods item in kvp.Value)
                        Console.WriteLine("Index:{0}, Size={1}, Value={2}", item.Index, item.Weight, item.Value);
                    Console.WriteLine();
                }
            else
                Console.WriteLine("无解");

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
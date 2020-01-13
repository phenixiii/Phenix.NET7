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

            Console.WriteLine("准备测试数据");
            int[] sizes = new int[] {2, 2, 6, 5, 4}; //物品规格
            int[] values = new int[] {6, 3, 5, 4, 6}; //物品价值
            List<IGoods> goodsList = new List<IGoods>();
            for (int i = 0; i < sizes.Length; i++)
                goodsList.Add(new Goods(i, sizes[i], values[i]));
            foreach (Goods item in goodsList)
                Console.WriteLine("Index:{0}, Size={1}, Value={2}", item.Index, item.Weight, item.Value);
            Console.WriteLine("物品数量={0}", goodsList.Count);
            Console.WriteLine();

            Console.WriteLine("**** 测试算法 ****");
            Console.WriteLine("背包knapsackSize={0}", 10);
            foreach (Goods item in ZeroOneKnapsackProblem.Pack(goodsList, 10))
                Console.WriteLine("Index:{0}, Size={1}, Value={2}", item.Index, item.Weight, item.Value);
            Console.Write("请按任意键继续");
            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("请按回车键结束演示");
            Console.ReadLine();
        }
    }
}
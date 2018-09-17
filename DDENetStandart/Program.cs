using System;
using System.Threading;
using System.Collections.Generic;

namespace DDENetStandart
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = new List<int>();
            bool flag = true;

            var t1 = new Thread(() =>
            {
                while (flag)
                { 
                    lock (list)
                    {
                        foreach(var item in list)
                        {
                            Console.WriteLine($"Item: {item}");
                        }
                        list.Clear();
                    }
                    Thread.Sleep(1000);
                }
            });
            t1.Start();
            var t2 = new Thread(() =>
            {
                var random = new Random();
                while (flag)
                {
                    lock (list)
                    {
                        list.Add(random.Next());
                    }
                    Thread.Sleep(100);
                }
            });
            t2.Start();



            Console.Read();
            flag = false;
            t1.Join();
            t2.Join();
        }
    }
}

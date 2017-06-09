using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalApproach.Framework
{
    using AppFunc = Func<EnvironmentVar, Task>;
    class TestMidWare
    {
        public string a;
        public List<string> lista;
        public AppFunc _next;

        public TestMidWare(string s, AppFunc next)
        {
            a = s;
            _next = next;
            lista = new List<string>();
            lista.Add(s);
        }
        public async Task<int> Invoke(EnvironmentVar enVar)
        {
            Task t = Task.Run(() =>
            {
                for (int i = 0; i >= 3; i--)
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Starts in: {0}-{1}",i,a);
                }
            });
            await t;
            await _next(enVar);
            Console.WriteLine("All tasks are done");

            return 0;
        }
    }
}

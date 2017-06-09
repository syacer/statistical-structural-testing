using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StatisticalApproach_GA
{
    using AppFunc = Func<EnvironmentVar, Task>;

    class TaskDistributor : App
    {
        public IAppBuilder[] apps;
        private EnvironmentVar[] _enVars;
        private int _numOfTasks;
        private Record _record;
        private int _numOfThread;
        public AppFunc _next;

        public TaskDistributor(EnvironmentVar[] enVars, int numOfTasks, Record record, int numOfThreads, AppFunc next)
        {
            _next = next;
            _numOfTasks = numOfTasks;
            apps = new IAppBuilder[numOfTasks];
            _record = record;
            _numOfThread = numOfThreads;
            _enVars = enVars;

        }
        public async Task<int> Invoke(EnvironmentVar enVar)
        {
            int k = 0;
            for (int i = 0; i < _enVars.Length; i++)
            {
                _enVars[i] = new EnvironmentVar();
                _enVars[i].initializeEnvironmentVar(i);
                _enVars[i].pmProblem = _record.sutInfo;
                apps[i] = new App();
            }
            Task<int>[] taskList = new Task<int>[_numOfThread];
            _next(null);   // Start Monitor
            _record.warmUpWatch.Stop();
            _record.gaWatch.Start();
            while (k < _numOfTasks)
            {
                if (_numOfTasks - k < _numOfThread)
                {
                    _numOfThread = _numOfTasks - k;
                }
                for (int i = 0; i < _numOfThread; i++)
                {
                    apps[k + i].Use<InitializationMidWare>();
                    apps[k + i].Use<StdGA>();
                    taskList[i] = apps[k + i].GetAppFunc().Invoke(_enVars[k + i]);
                    //taskList[i] = Task.Run(()=>{
                    //    Console.WriteLine("in tasks {0}",Task.CurrentId);
                    //    System.Threading.Thread.Sleep(5000);
                    //    Console.WriteLine("finish task {0}", Task.CurrentId);

                    //});
                }
                //while (!taskList.All(x => x.Result == 1)) ;
                for (int i = 0; i < _numOfThread; i++)
                {
                    await taskList[i];
                }
                k = k + _numOfThread;
            }
            _record.gaWatch.Stop();
            Console.WriteLine("All tasks are done");
            return 0;
        }
    }
}

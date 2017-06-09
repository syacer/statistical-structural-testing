using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalApproach_GA
{
    public interface IAppBuilder
    {
        void Use<T>(params object[] args);
        Func<EnvironmentVar, Task<int>> GetAppFunc();
    }
    public class App : IAppBuilder
    {
        Queue<object> _queue;
        public Func<EnvironmentVar, Task<int>> funcTask;
        public App()
        {
            _queue = new Queue<object>();
            funcTask += InvokeNext;
        }

        public Func<EnvironmentVar, Task<int>> GetAppFunc()
        {
            return funcTask;
        }
        public void Use<T>(params object[] args)
        {
            Type type = typeof(T);
            var totalArgs = new object[args.Count() + 1];
            for (int i = 0; i < args.Count(); i++)
            {
                totalArgs[i] = args[i];
            }
            totalArgs[args.Count()] = funcTask;
            var newTObj = Activator.CreateInstance(type, totalArgs);
            _queue.Enqueue(newTObj);
            //FieldInfo[] fields = type.GetFields();
            //foreach (var field in fields)
            //{
            //    string name = field.Name; 
            //    object temp = field.GetValue(newTObj);
            //}

        }
        public Task<int> InvokeNext(EnvironmentVar env)
        {
            try
            {
                if (_queue.Count == 0)
                {
                    env.emptyQueue = true;

                    return Task.Run<int>(()=> 
                    {
                        return 0;
                    });
                }
                var Obj = _queue.Dequeue();
                MethodInfo InvokeMethod = Obj.GetType().GetMethod("Invoke");
                return (Task<int>)InvokeMethod.Invoke(Obj, new object[] { env });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}

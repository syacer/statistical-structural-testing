using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeterministicApproach_GA
{
    using AppFunc = Func<EnvironmentVar, Task>;
    class InitializationMidWare : App
    {
        public AppFunc _next;

        public InitializationMidWare(AppFunc next)
        {
            _next = next;
        }
        public async Task<int> Invoke(EnvironmentVar enVar)
        {
            Task taskInitialPop = new Task(()=>
            {
                foreach (var genotype in enVar.tempPopulation)
                {
                    for (int i = 0; i < enVar.pmGenotypeLen; i++)
                    {
                        int count = (int)enVar.pmProblem["Dimension"];
                        int[] newGene = new int[count];
                        for (int j = 0; j < count; j++)
                        {
                            newGene[j] = enVar.rnd.Next(
                                ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[j].Item1,
                                ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[j].Item2);
                        }
                        genotype.Key.ModifyGenValue(i, newGene);
                    }
                }
            });

            taskInitialPop.Start();
            await taskInitialPop;
            Console.WriteLine("Next GA");
            await _next(enVar);
            Console.WriteLine("Task finish {0}",Task.CurrentId);
            return 1;
        }
    }
}

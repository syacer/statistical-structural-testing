using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalApproach_GA
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
                        if (count != 2)
                        {
                            Console.WriteLine("Dimension is not 2, Program should stop!");
                            break;
                        }
                        int[] newGene = new int[3];
                        int decodeLow = 0;
                        int decodeHigh = 0;

                        decodeLow = (int)Math.Pow((((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[0].Item1), 2)
                                         + (((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[0].Item1);
                        decodeHigh = (int)Math.Pow((((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[1].Item2), 2)
                                         + (((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[1].Item2);
                        int lowBound = enVar.rnd.Next(decodeLow, decodeHigh);
                        int highBound = enVar.rnd.Next(lowBound, decodeHigh);

                        newGene[0] = lowBound;
                        newGene[1] = highBound;
                        newGene[2] = enVar.rnd.Next(1, (int)enVar.pmProblem["Weight"]);
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

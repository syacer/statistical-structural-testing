
using System.Collections.Generic;
namespace DeterministicApproach_GA
{
    using Pool = Dictionary<Genotype<GeneD2>, double>;
    using GeneType = GeneD2;
    using System.Diagnostics;
    using System;
    public class EnvironmentVar
    {
        /** Monitor**/
        public int taskNum;
        public bool finishIndicate = false;
        public bool continueIndicate = true;
        /** EndOfMonitor**/

        public int pmPopSize = 100;
        public int pmGenotypeLen = 200;
        public double pmCrossOverRate = 0.8;
        public double pmMutationRate = 0.2;
        public double pmK = 0.5;                    //[0.5,1.5]
        public Pool population;
        public Pool tempPopulation;
        public bool emptyQueue;
        public Dictionary<string, object> pmProblem;
        public object[] genFitRecord = new object[2] {0,0};
        public Random rnd = new Random(Guid.NewGuid().GetHashCode());

        public void initializeEnvironmentVar(int taskNumber)
        {
            taskNum = taskNumber;
            emptyQueue = false;
            population = new Pool();
            tempPopulation = new Pool();
            for (int i = 0; i < pmPopSize; i++)
            {
                Genotype<GeneType> genotype = new Genotype<GeneType>(pmGenotypeLen);
                tempPopulation.Add(genotype, -10.0);
                population.Add(genotype, -10.0);
            }
        }
    }
}

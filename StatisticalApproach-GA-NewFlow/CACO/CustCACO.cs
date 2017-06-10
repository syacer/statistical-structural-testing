using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StatisticalApproach.Framework;

#if false
namespace StatisticalApproach.CACO
{
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.Distributions;
    using Utility;


    using AppFunc = Func<EnvironmentVar, Task>;
    using CEPool = Dictionary<CACO3, double[]>;

    class CustCACO
    {

        public AppFunc _next;
        private EnvironmentVar enVar;

		int maxGen = 200;
        int initialtestsize = 300;
        int[] testSizeOfCEx;
        int timeoutT = 1000;
        public double paramZeta = 0.3;
        public double paramQ = 0.2;
        public int poolSize = 100;
        public int numOfTheta = 10;
        public int dimension = 2;
        public int numOfRecordHistory = 30;
        public int[] disQualifyCEAry;
        public int[] usedgranttestinputs;
        public double[] estCExSizeAry;
        public Queue<double>[] ceFitHistoryAry;
        public CEPool[] cePool;
        public double[] fitThreshold = new double[] {0.95}; //IMPORTANT This is only for CALDAY SUT
        private Record _record;
        private int _runIndex = 0;
        int gen = 0;

        public CustCACO(Record record, int runIndex, AppFunc next)
        {
            _next = next;
            _record = record;
            _runIndex = runIndex;
        }

        public async Task<int> Invoke(EnvironmentVar envar)
        {
            enVar = envar;
            dimension = (int)enVar.pmProblem["Dimension"];
            await Task.Run(() => CACO_Start());
            await _next(enVar);
            return 0;
        }

        public void CACO_Initialization()
        {
		   maxGen = (int)enVar.pmProblem["MaxGen"];
		   initialtestsize = (int)enVar.pmProblem["TestSize"];
		   testSizeOfCEx = new int[(int)enVar.pmProblem["NumOfCE"]];
            for (int i = 0; i < (int)enVar.pmProblem["NumOfCE"]; i++)
            {
                testSizeOfCEx[i] = initialtestsize;
            }
			
           disQualifyCEAry = new int[(int)enVar.pmProblem["NumOfCE"]];
           usedgranttestinputs = new int[(int)enVar.pmProblem["NumOfCE"]];
           estCExSizeAry = new double[(int)enVar.pmProblem["NumOfCE"]];
           ceFitHistoryAry = new Queue<double>[(int)enVar.pmProblem["NumOfCE"]];
           cePool = new CEPool[(int)enVar.pmProblem["NumOfCE"]];
		   

            for (int i = 0; i < ceFitHistoryAry.Length; i++)
            {
                ceFitHistoryAry[i] = new Queue<double>();
            }
           for (int k = 0; k < cePool.Length; k++)
            {
                cePool[k] = new CEPool();
                for (int i = 0; i < poolSize; i++)
                {
                    CACO3 tmpSol = new CACO3(numOfTheta, dimension,k);
                    tmpSol.Randomize(enVar);
                    double[] tmpFit = new double[] { -10, -10 };
                    cePool[k].Add(tmpSol, tmpFit);
                }
            }
            for (int i = 0; i < disQualifyCEAry.Length; i++)
            {
                disQualifyCEAry[i] = 1;
            }
        }

        public void CACO_Start()
        {
            LocalFileAccess lfa = new LocalFileAccess();
            _record.Watch[_runIndex].Start();
            CACO_Initialization();
            int[] token = new int[1];
            Console.WriteLine("In Fitness Evaluation...");
            CACO_FitnessEvaluation(token);   //Find unqualied ce, find out test size, evaluate those
            while (token[0] == 0) ;
            token[0] = 0;
            Console.WriteLine("In Update Ranks, Parameters Update...");
            CACO_UpdateRanks();   // Update ranks for unqualified ce
            CACO_ParametersUpdate();  // Update the parameters for unqualified ce

            // Update Records
            List<string> generation = new List<string>() { gen.ToString() };
            List<string> currentCElist = new List<string>();
            List<string> currentBestSolution = new List<string>();
            List<double> currentFitnessList = new List<double>();
            Console.WriteLine("Generation: {0}", gen);
            for (int i = 0; i < cePool.Length; i++)
            {
                string dataOut = "CE: " + i.ToString() + ", " + "Fit: " + ceFitHistoryAry[i].Last().ToString();
                currentCElist.Add(dataOut);
                currentFitnessList.Add(ceFitHistoryAry[i].Last());
                Console.WriteLine(dataOut);
            }
            // Output CurrentBest Result;
            for (int i = 0; i < (int)enVar.pmProblem["NumOfCE"]; i++)
            {
                CACO3 currentBest = cePool[i].Keys.Where(x => x.rank == 1).First();
                _record.currentBestSolution[_runIndex][i] = Print_Solution(currentBest);
            }

            _record.currentGen[_runIndex] = generation;
            _record.currentCElist[_runIndex] = currentCElist;
            _record.currentFitnessList[_runIndex] = currentFitnessList;
            _record.updateIndicate[_runIndex] = true;
            while (_record.updateIndicate[_runIndex] == true) ;

            // Start C-ACOs
            while (gen < maxGen)
            {
                gen = gen + 1;
                if (disQualifyCEAry.Sum() == 0)
                {
                    var a = cePool[0].First(x => x.Key.rank == 1);
                    break;
                }
                Console.WriteLine("In Solution Construction...");
                CACO_SolutionConstruction();
                Console.WriteLine("In Fitness Evaluations...");
                CACO_FitnessEvaluation(token);
                while (token[0] == 0) ;
                token[0] = 0;
                Console.WriteLine("In Rank update and parameter update...");
                CACO_UpdateRanks();
                CACO_ParametersUpdate();

                Array.Clear(disQualifyCEAry, 0, disQualifyCEAry.Length);
                for (int i = 0; i < (int)enVar.pmProblem["NumOfCE"]; i++)
                {
                    disQualifyCEAry[2] = 0;  //IMPORTANT: THIS IS ONLY FOR calday
                    disQualifyCEAry[6] = 0; //IMPORTANT: THIS IS ONLY FOR calday
                    if (ceFitHistoryAry[i].Last() < fitThreshold[0])
                    {
                        disQualifyCEAry[i] = 1;
                    }
                    else
                    {
                       if (usedgranttestinputs[i] == 0)
                       {
                            int worstPerformanceCEIndex = cePool.OrderBy(x => x.Values.Average(y => y[(int)enVar.evaToken]))
                                .Where(x => x.Keys.First().ceIndex != 2)  //IMPORTANT This is only for CALDAY SUT
                                .Where(x => x.Keys.First().ceIndex != 6) //IMPORTANT This is only for CALDAY SUT
                                .First()
                                .Keys.First().ceIndex;
                            testSizeOfCEx[i] = 0;
                            testSizeOfCEx[worstPerformanceCEIndex] = testSizeOfCEx[worstPerformanceCEIndex] + initialtestsize;
                            usedgranttestinputs[i] = 1;
                        }
                    }
                    if (ceFitHistoryAry[i].Count > numOfRecordHistory)
                    {
                        ceFitHistoryAry[i].Dequeue();
                    }
                }

                // Update Records
                generation = new List<string>() { gen.ToString() };
                currentCElist = new List<string>();
                currentBestSolution = new List<string>();
                currentFitnessList = new List<double>();
                Console.WriteLine("Generation: {0}", gen);
                for (int i = 0; i < cePool.Length; i++)
                {
                    string dataOut = "CE: " + i.ToString() + ", " + "Fit: " + ceFitHistoryAry[i].Last().ToString();
                    currentCElist.Add(dataOut);
                    currentFitnessList.Add(ceFitHistoryAry[i].Last());
                    Console.WriteLine(dataOut);
                }
                // Output CurrentBest Result;
                for (int i = 0; i < (int)enVar.pmProblem["NumOfCE"]; i++)
                {
                    CACO3 currentBest = cePool[i].Keys.Where(x => x.rank == 1).First();
                    _record.currentBestSolution[_runIndex][i] = Print_Solution(currentBest);
                }

                _record.currentGen[_runIndex] = generation;
                _record.currentCElist[_runIndex] = currentCElist;
                _record.currentFitnessList[_runIndex] = currentFitnessList;
                _record.updateIndicate[_runIndex] = true;           
                while (_record.updateIndicate[_runIndex] == true);
            }
            _record.Watch[_runIndex].Stop();
            enVar.finishIndicate = true;
        }

        public List<string> Print_Solution(CACO3 solution)
        {
            string theta = null;
            string weight = null;

            for (int i = 0; i < solution.thetas.Length; i++)
            {
                string tmp = "(";
                for (int j = 0; j < dimension; j++)
                {
                    tmp = tmp + Math.Round(solution.thetas[i][j],2) + " ";
                }
                tmp.Remove(tmp.Length-1,1);
                tmp = tmp + ")";
                theta = theta + tmp + " ";
            }
            theta.Remove(theta.Length - 1, 1);

            for (int i = 0; i < solution.weights.Length; i++)
            {
                string tmp = "[";
                for (int j = 0; j < dimension; j++)
                {
                    tmp = tmp + Math.Round(solution.weights[i][j],2) + " ";
                }
                tmp.Remove(tmp.Length - 1, 1);
                tmp = tmp + "]";
                weight = weight + tmp + " ";
            }
            weight.Remove(weight.Length - 1, 1);

            return new List<string>() {theta, weight };
        }
        public void CACO_ParametersUpdate()
        {
            for (int k = 0; k < cePool.Length; k++)
            {
                if (disQualifyCEAry[k] != 1)
                {
                    continue;
                }
                for (int i = 0; i < poolSize; i++)
                {
                    cePool[k].ElementAt(i).Key.PheromoneUpdate(paramZeta, cePool[k]);
                    cePool[k].ElementAt(i).Key.GaussianWeightUpdate(paramQ, poolSize);
                }
            }
        }

        public void CACO_SolutionConstruction()
        {
            List<double[]> gaussians = new List<double[]>();
            CEPool[] tmpPool = new CEPool[(int)enVar.pmProblem["NumOfCE"]];

            for (int i = 0; i < tmpPool.Length; i++)
            {
                tmpPool[i] = new CEPool();
            }

            double[] lowbounds = new double[dimension];
            double[] highbounds = new double[dimension];
            double[] weightlowbound = new double[dimension];
            double[] weighthighbound = new double[dimension];

            for (int o = 0; o < dimension; o++)
            {
                lowbounds[o] = ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[o].Item1;
                highbounds[o] = ((List<Tuple<int, int>>)enVar.pmProblem["Bound"])[o].Item2;
                weightlowbound[o] = 0;
                weighthighbound[o] = numOfTheta + 1;
            }

            for (int m = 0; m < cePool.Length; m++)
            {
                if (disQualifyCEAry[m] != 1)
                {
                    tmpPool[m] = cePool[m];
                    continue;
                }
                var tmp = Copy.DeepCopy(cePool[m].OrderByDescending(x => x.Value[enVar.evaToken]).First());

                for (int i = 0; i < poolSize; i++)  // Generate a pool of individuals
                {
                    tmpPool[m].Add(new CACO3(numOfTheta, dimension, m), new double[] { -10, -10 });

                    int selectedGaussIndex = -1;

                    // First, Select a Gaussian Component
                    double lowIndex = 0.0;
                    double sumOfWeight = cePool[m].Sum(x => x.Key.omega);
                    double rndNum = enVar.rnd.NextDouble() * sumOfWeight;
                    for (int j = 0; j < poolSize; j++)
                    {
                        if (rndNum >= lowIndex && rndNum <= lowIndex + cePool[m].ElementAt(j).Key.omega)
                        {
                            selectedGaussIndex = j;
                            break;
                        }
                        lowIndex = lowIndex + cePool[m].ElementAt(j).Key.omega;
                    }
                    CACO3 selectedSol = cePool[m].ElementAt(selectedGaussIndex).Key;


                    Vector<double> constTheta = Vector.Build.Dense(lowbounds);
                    Vector<double> constWeight = Vector.Build.Dense(weightlowbound);
                    Vector<double> remain = Vector.Build.Dense(weighthighbound);

                    for (int k = 0; k < numOfTheta; k++)
                    {
                        // Second, Generate new Theta at k'th Column
                        Vector<double> meanOfTheta = selectedSol.thetas[k];
                        Vector<double> varOfTheta = selectedSol.varThetas[k];
                        Vector<double> sampleTheta = Vector.Build.Dense(dimension);
                        int timeoutCounter = 0;

                        do
                        {
                            sampleTheta = TruncatedGaussianSampling(
                                meanOfTheta, varOfTheta, constTheta,
                                Vector.Build.Dense(lowbounds),
                                Vector.Build.Dense(highbounds)
                            );

                            if (timeoutCounter >= timeoutT)
                            {
                                sampleTheta = Vector.Build.Dense(highbounds);
                                // Console.WriteLine("TimeoutCount at ~309");
                            }
                            timeoutCounter = timeoutCounter + 1;
                        }while ((sampleTheta - Vector.Build.Dense(highbounds)).Any(x => x > 0));

                        constTheta = sampleTheta;
                        tmpPool[m].ElementAt(i).Key.thetas[k] = sampleTheta;

                        // Third, Generate new Weight at k'th Column
                        Vector<double> meanOfWeight = selectedSol.weights[k];
                        Vector<double> varOfWeight = selectedSol.varWeights[k];
                        Vector<double> sampleWeight = Vector.Build.Dense(dimension);

                        constWeight = 2 * meanOfWeight - remain;
                        Vector<double> realSampleWeight = null;
                        timeoutCounter = 0;

                        do
                        {
                            sampleWeight = TruncatedGaussianSampling(
                                meanOfWeight, varOfWeight, constWeight,
                                Vector.Build.Dense(weightlowbound),
                                Vector.Build.Dense(weighthighbound)
                            );
                            realSampleWeight = 2 * meanOfWeight - sampleWeight;

                            if (timeoutCounter >= timeoutT)
                            {
                                //Console.WriteLine("TimeoutCount at ~231");
                                realSampleWeight.Clear();
                            }
                            timeoutCounter = timeoutCounter + 1;
                        } while (realSampleWeight.Any(x => x < 0));

                        tmpPool[m].ElementAt(i).Key.weights[k] = realSampleWeight;
                        remain = remain - realSampleWeight;
                    }

                    // The last block, its weight is: Sum - used weights
                    Vector<double> sumVect = Vector.Build.Dense(dimension);
                    foreach (Vector<double> t in tmpPool[m].ElementAt(i).Key.weights)
                    {
                        sumVect = sumVect + t;
                    }
                    if (sumVect.Any(x => x > numOfTheta + 1))
                    {
                        int DEBUG = 1;
                    }
                   tmpPool[m].ElementAt(i).Key.weights[numOfTheta] = sumVect.SubtractFrom(numOfTheta + 1);
                }
                //tmpPool[m].Add(tmp.Key, tmp.Value);
            }
            cePool = tmpPool;
        }

        public Vector<double> TruncatedGaussianSampling(Vector<double> mean, Vector<double> var, Vector<double> constraint, Vector<double> lowBound, Vector<double> highBound)
        {
            double[] meanArray = mean.ToArray();
            double[] varArray = var.ToArray();
            Vector<double> Sample = Vector.Build.Dense(dimension);

            for (int i = 0; i < dimension; i++)
            {
                double NormalizedConstraint = (constraint[i] - mean[i]) / var[i];

                if (Math.Abs(NormalizedConstraint) < 2)
                {
                    bool accept = false;
                    double xStar = 1;
                    double M = -1;

                    // Generate test case from Z~N(0,1)I(Z>=(C-M)/theta)
                    if (xStar >= NormalizedConstraint)
                    {
                        M = Math.Pow(Math.E, 0.5 - (constraint[i] - mean[i]) / var[i]);
                    }
                    else if (xStar < NormalizedConstraint)
                    {
                        M = Math.Pow(Math.E, -0.5 * Math.Pow((constraint[i] - mean[i]) / var[i], 2));
                    }

                    int timeoutCounter = 0;
                    while (accept == false)
                    {
                        double sample = Exponential.Sample(enVar.rnd, 1) + NormalizedConstraint;
                        double u = ContinuousUniform.Sample(0.0, 1.0);
                        double pfOfe = Math.Pow(Math.E, -0.5 * Math.Pow(sample, 2));
                        double pgOfe = Math.Pow(Math.E, -(sample - NormalizedConstraint));
                        double result = pfOfe / (pgOfe * (M + 0.001));
                        if (u <= result)
                        {
                            //De-normalize sample
                            sample = sample * var[i] + mean[i];
                            Sample[i] = sample;
                            accept = true;
                        }
                        if (timeoutCounter >= timeoutT)
                        {
                            Sample[i] = constraint[i];
                            accept = true;
                            // Console.WriteLine("TimeoutCount at ~309");
                        }
                        timeoutCounter = timeoutCounter + 1;
                    }
                }
                else if ((constraint[i] - mean[i]) < 0) // Using Gaussian sampling
                {
                    bool accept = false;
                    int timeoutCounter = 0;
                    while (accept == false)
                    {
                        double sample = Normal.Sample(enVar.rnd, mean[i], var[i]);
                        if (sample > constraint[i])
                        {
                            Sample[i] = sample;
                            accept = true;
                        }
                        if (timeoutCounter >= timeoutT)
                        {
                            Sample[i] = constraint[i];
                            accept = true;
                            //Console.WriteLine("TimeoutCount at ~335");

                        }
                        timeoutCounter = timeoutCounter + 1;
                    }
                }
                else
                {
                    Sample[i] = ContinuousUniform.Sample(enVar.rnd, constraint[i], mean[i]+highBound[i]);
                }
            }
            return Sample;
        }

        public void CACO_UpdateRanks()
        {
            for (int i = 0; i < disQualifyCEAry.Length; i++)
            {
                if (disQualifyCEAry[i] == 1)
                {
                    double[] bestFit = new double[2];
                    var tmpPool = cePool[i].OrderByDescending(x => x.Value[enVar.evaToken]);
                    for (int k = 0; k < poolSize; k++)
                    {
                        tmpPool.ElementAt(k).Key.SolutionRankUpdate(k + 1);
                        if (k == 0)
                        {
                            bestFit[0] = tmpPool.ElementAt(k).Value[0];
                            bestFit[1] = tmpPool.ElementAt(k).Value[1];
                        }
                    }

                    ceFitHistoryAry[i].Enqueue(bestFit[enVar.evaToken]);
                }
            }
        }
        public async void CACO_FitnessEvaluation(int[] token)
        {
            List<Task> lTasks = new List<Task>();
            int remainTasks = poolSize;
            for (int j = 0; j < disQualifyCEAry.Length; j++)
            {
                Console.Write("{0} ", j);
                if (disQualifyCEAry[j] == 1)
                {
                    // Test inputs Generation
                    // Fitness Evaluation
                    for (int k = 0; k < poolSize; k++)
                    {
                        //Console.WriteLine("{0}'th genotype fitness assement", k);
                        CACO3 caco = cePool[j].ElementAt(k).Key;
                        //caco.GetEstSizeAndTestSizeOfCEx(testSizeOfCEx);
                        //cePool[j][caco][0] = caco.CalEstFitness(enVar);
                        //cePool[j][caco][1] = caco.CalTrueFitness(enVar);
                        lTasks.Add(Task.Run(() =>
                        {
                            caco.GetEstSizeAndTestSizeOfCEx(testSizeOfCEx[j]);
                            cePool[j][caco][0] = caco.CalEstFitness(enVar);
                            //cePool[j][caco][1] = caco.CalTrueFitness(enVar);
                            // Console.WriteLine("{0}'th finished assement", k);
                        }));
                        if (lTasks.Count == 6 || (poolSize - k - 1 < 6))
                        {
                            for (int i = 0; i < lTasks.Count; i++)
                            {
                                await lTasks[i];
                            }
                            lTasks.Clear();
                        }
                    }
                }
            }
            token[0] = 1;
            //Console.WriteLine("Fitness Evaluation Done");
        }
    }
}
#endif
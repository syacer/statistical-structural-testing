using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalApproach.Framework
{
    public class Record
    {
        public List<string>[] currentGen;
        public List<string>[] currentCElist;
        public List<string>[][] currentBestSolution;
        public List<double>[] currentFitnessList;
        public bool[] updateIndicate;
        public bool[] updateDisplay;
        public Stopwatch[] Watch;
        public Dictionary<string, object> sutInfo;
        public string bestSolution;

        private int _numOfRuns = -1;
        public Record(int numOfRuns)
        {
            updateIndicate = new bool[numOfRuns];
            updateDisplay = new bool[numOfRuns];
            Watch = new Stopwatch[numOfRuns];
            _numOfRuns = numOfRuns;
            for (int i = 0; i < numOfRuns; i++)
            {
                updateIndicate[i] = new bool();
                updateDisplay[i] = new bool();
                currentGen = new List<string>[numOfRuns];
                currentCElist = new List<string>[numOfRuns];
                currentBestSolution = new List<string>[numOfRuns][];
                currentFitnessList = new List<double>[numOfRuns];

                Watch[i] = new Stopwatch();
            }
        }

        public void InitializeCurrentBest(int numOfCE)
        {
            for (int k = 0; k < _numOfRuns; k++)
            {
                currentBestSolution[k] = new List<string>[numOfCE];
            }
        }
    }
}    // Refresh Current Generation for each Run, update table, set update array,set continue to true

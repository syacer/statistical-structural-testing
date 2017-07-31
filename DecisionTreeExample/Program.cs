using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math.Optimization.Losses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTreeExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // In this example, we will learn a decision tree directly from integer
            // matrices that define the inputs and outputs of our learning problem.

            int[][] inputs =
            {
                new int[] { 0, 0 },
                new int[] { 0, 1 },
                new int[] { 1, 0 },
                new int[] { 1, 1 },
            };

            int[] outputs = // xor between inputs[0] and inputs[1]
            {
                0, 2, 1, 2
            };

            // Create an ID3 learning algorithm
            C45Learning teacher = new C45Learning();
            DecisionVariable var1 = new DecisionVariable("0",new Accord.DoubleRange(0,999));
            DecisionVariable var2 = new DecisionVariable("1", new Accord.DoubleRange(0, 999));
            var1.Nature = DecisionVariableKind.Continuous;
            var2.Nature = DecisionVariableKind.Continuous;
            teacher.Attributes.Add(var1);
            teacher.Attributes.Add(var2);

            // Learn a decision tree for the XOR problem
            var tree = teacher.Learn(inputs, outputs);
            var r = tree.ToRules();
            for (int i = 0; i < r.Count; i++)
            {
                double o = r.ElementAt(i).Output;
                string name1 = r.ElementAt(i).Variables.ElementAt(0).Name;
                string name2 = r.ElementAt(i).Variables.ElementAt(1).Name;
            }
            // Compute the error in the learning
            double error = new ZeroOneLoss(outputs).Loss(tree.Decide(inputs));

            // The tree can now be queried for new examples:
            int[] predicted = tree.Decide(inputs); // should be { 0, 1, 1, 0 }

        }
    }
}


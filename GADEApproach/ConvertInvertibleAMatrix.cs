using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
    static class ConvertInvertibleAMatrix
    {
        static public void CreateInvertibleAMatrix(
            ref Matrix<double> AMatrix,
            int[] binsInSets,
            double[][] binsSetup,
            out int[] newBinsInSets,
            int numOfLabels
            )
        {
            List<Vector<double>> newColumnVects = new List<Vector<double>>();
            newBinsInSets = new int[binsInSets.Length];
            List<Vector<double>> OrthUnitVects = new List<Vector<double>>();

            try
            {
                for (int i = 0; i < numOfLabels; i++)
                {
                    int[] binIndicesOfithLabel = binsInSets.Select((x, j) =>
                    {
                        if (x == i + 1)
                        {
                            return j;
                        }
                        return -1;
                    }).Where(x => x != -1).ToArray();
                    if (binIndicesOfithLabel.Length == 0)
                    {
                        continue;
                    }
                    Vector<double> triProb = Vector<double>.Build.Dense(numOfLabels);
                    for (int j = 0; j < binIndicesOfithLabel.Length; j++)
                    {
                        triProb += Vector<double>.Build.Dense(binsSetup[binIndicesOfithLabel[j]]);
                    }
                    triProb = triProb.Divide(binIndicesOfithLabel.Length);
                    Vector<double> ei = Copy.DeepCopy(triProb);
                    if (OrthUnitVects.Count == 0)
                    {
                        double l2Norm = ei.L2Norm();
                        ei = ei.Divide(l2Norm);
                        newColumnVects.Add(triProb);
                        OrthUnitVects.Add(ei);
                        for (int j = 0; j < binsInSets.Length; j++)
                        {
                            if (binsInSets[j] == i + 1)
                            {
                                newBinsInSets[j] = 1;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < OrthUnitVects.Count - 1; j++)
                        {
                            ei -= OrthUnitVects[j].Multiply(triProb.DotProduct(OrthUnitVects[j]));
                        }
                        ei /= ei.L2Norm();
                        //Console.WriteLine("A:{0}, {1}",i,Math.Abs(ei.DotProduct(OrthUnitVects[OrthUnitVects.Count - 1])));
                        if (Math.Abs(ei.DotProduct(OrthUnitVects[OrthUnitVects.Count - 1])) >= 0.9999
                            && Math.Abs(ei.DotProduct(OrthUnitVects[OrthUnitVects.Count - 1])) <= 1.0001)
                        {
                            Vector<double> triProb2 = newColumnVects[newColumnVects.Count - 1];
                            int newNumOfBins = newBinsInSets.Count(x => x == OrthUnitVects.Count - 1 + 1);
                            Vector<double> newTriProb = (triProb.Multiply(binIndicesOfithLabel.Length)
                                + triProb2.Multiply(newNumOfBins))
                                .Divide(binIndicesOfithLabel.Length + newNumOfBins);
                            newColumnVects[newColumnVects.Count - 1] = newTriProb;
                            ei = Copy.DeepCopy(newTriProb);
                            for (int j = 0; j < OrthUnitVects.Count - 1; j++)
                            {
                                ei -= OrthUnitVects[j].Multiply(newTriProb.DotProduct(OrthUnitVects[j]));
                            }
                            ei /= ei.L2Norm();
                            OrthUnitVects[OrthUnitVects.Count - 1] = ei;
                            for (int j = 0; j < binsInSets.Length; j++)
                            {
                                if (binsInSets[j] == i + 1)
                                {
                                    newBinsInSets[j] = OrthUnitVects.Count;
                                }
                            }
                        }
                        else
                        {
                            newColumnVects.Add(triProb);
                            for (int j = 0; j < binsInSets.Length; j++)
                            {
                                if (binsInSets[j] == i + 1)
                                {
                                    newBinsInSets[j] = OrthUnitVects.Count + 1;
                                }
                            }
                            OrthUnitVects.Add(ei);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (newColumnVects.Count == 0)
            {
                Console.WriteLine("EEEEEEEE");
            }
            Matrix<double> InvertibleAMatrix = Matrix<double>
                .Build.Dense(numOfLabels,newColumnVects.Count);
            for (int i = 0; i < newColumnVects.Count; i++)
            {
                InvertibleAMatrix.SetColumn(i,newColumnVects[i]);
            }
            AMatrix = InvertibleAMatrix;
            //int rank = AMatrix.Rank();
            //var tmpp = newBinsInSets.Distinct().OrderBy(x=>x);
            //int tmp = OrthUnitVects.Count;
           // var a = InvertibleAMatrix.TransposeThisAndMultiply(InvertibleAMatrix).Inverse();
        }
    }
}

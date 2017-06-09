using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalApproach_GA
{
    interface Gene
    {
        int GetGenValue(ref int[] gene);
        int SetGenValue(params int[] values);
    }
    [Serializable]
    public class GeneD1 : Gene
    {
        int[] gen;

        public GeneD1()
        {
            gen = new int[1];
            gen[0] = -1;
        }

        public int GetGenValue(ref int[] gene)
        {
            gene = new int[1];
            gene[0] = gen[0];
            return 0;
        }

        public int SetGenValue(params int[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                gen[i] = values[i];
            }

            return 0;
        }
    }

    [Serializable]
    public class GeneD2 : Gene
    {
        int[] gen;

        public GeneD2()
        {
            gen = new int[2];
            gen[0] = -1;
            gen[1] = -1;
        }

        public int GetGenValue(ref int[] gene)
        {
            gene = new int[2];
            gene[0] = gen[0];
            gene[1] = gen[1];
            return 0;
        }

        public int SetGenValue(params int[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                gen[i] = values[i];
            }

            return 0;
        }
    }

    [Serializable]
    public class GeneD3 : Gene
    {
        int[] gen;

        public GeneD3()
        {
            gen = new int[3];
            gen[0] = -1;
            gen[1] = -1;
            gen[2] = -1;
        }

        public int GetGenValue(ref int[] gene)
        {
            gene = new int[3];
            gene[0] = gen[0];
            gene[1] = gen[1];
            gene[2] = gen[2];
            return 0;
        }
        public int GetGenValueByIndex(int index)
        {
            return gen[index];
        }

        public int SetGenValue(params int[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                gen[i] = values[i];
            }

            return 0;
        }
    }

    [Serializable]
    public class Genotype<T>
    {
        private T[] genotype;
        private int genLen = 0;

        public Genotype(int genotypeLenth)
        {
            genotype = new T[genotypeLenth];
            genLen = genotypeLenth;
            for (int i = 0; i < genLen; i++)
            {
                genotype[i] = (T)Activator.CreateInstance(typeof(T));
            }
        }

        public T[] GetGenotype()
        {
            return genotype;
        }
        public int ModifyGenValue(int index, params int[] genValue)
        {
            if (index > genLen - 1)
            {
                return -1;
            }

            string type = genotype[index].GetType().Name;
            switch (type)
            {
                case "GeneD1":
                    {
                        ((GeneD1)(object)genotype[index]).SetGenValue(genValue);
                        break;
                    }
                case "GeneD2":
                    {
                        ((GeneD2)(object)genotype[index]).SetGenValue(genValue);
                        break;
                    }
                case "GeneD3":
                    {
                        ((GeneD3)(object)genotype[index]).SetGenValue(genValue);
                        break;
                    }
                default:
                    {
                        System.Console.WriteLine("Other number");
                        break;
                    }
            }
            return 0;
        }

        public int[] GetGenotypeAtIndex(int i)
        {
            if (i > genLen - 1 || i < 0)
            {
                return null;
            }

            int[] geneValue = null;

            string type = genotype[i].GetType().Name;
            switch (type)
            {
                case "GeneD1":
                    {
                        ((GeneD1)(object)genotype[i]).GetGenValue(ref geneValue);
                        break;
                    }
                case "GeneD2":
                    {
                        ((GeneD2)(object)genotype[i]).GetGenValue(ref geneValue);
                        break;
                    }
                case "GeneD3":
                    {
                        ((GeneD3)(object)genotype[i]).GetGenValue(ref geneValue);
                        break;
                    }
                default:
                    {
                        System.Console.WriteLine("Other number");
                        break;
                    }
            }
            return geneValue;

        }
    }
}

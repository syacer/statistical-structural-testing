using GADEApproach.TrainditionalApproaches;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;

namespace GADEApproach
{
    public partial class Form1 : Form
    {
        List<int> numOfLablesArray = null;
        List<double[]> entropyPropotionArray = null;
        string rootPath = null;
        bool inputSimulatorOn = false;
        bool keyState = false;
        LocalFileAccess lfa;
        DataTable inputsBinDt = null;

        public Form1()
        {
            InitializeComponent();
            button4.BackColor = Color.Red;
            numOfLablesArray = new List<int>();
            entropyPropotionArray = new List<double[]>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lfa = new LocalFileAccess();
            if (!File.Exists(rootPath + @"\" + Environment.MachineName))
            {
                File.Create(rootPath + @"\" + Environment.MachineName).Close();
            }
            timer2.Enabled = true;
            Task.Run(() => new Experiments().ExperimentsA(numOfLablesArray.ToArray(), entropyPropotionArray.ToArray(), rootPath));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string numLables = textBox1.Text;
            string[] str = numLables.Split(';');
            
            for (int i = 0; i < str.Length; i++)
            {
                numOfLablesArray.Add(Convert.ToInt16(str[i]));
            }
            var itemSelected = checkedListBox1.CheckedItems;

            for (int i = 0; i < str.Length; i++)
            {
                var temp = new double[itemSelected.Count];
                for (int j = 0; j < itemSelected.Count; j++)
                {
                    temp[j] = Convert.ToDouble(itemSelected[j]);
                }
                entropyPropotionArray.Add(temp);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    rootPath = fbd.SelectedPath + @"\";
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Invalid Folder");
                }
            }
            textBox4.Text = rootPath;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (inputSimulatorOn == false)
            {
                timer1.Enabled = true;
                button4.BackColor = Color.Green;
                inputSimulatorOn = true;
            }
            else
            {
                inputSimulatorOn = false;
                timer1.Enabled = false;
                button4.BackColor = Color.Red;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            InputSimulator ism = new InputSimulator();
            if (keyState == false)
            {
                ism.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.VK_Z);
                keyState = true;
            }
            else
            {
                keyState = false;
                ism.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.BACK);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            lfa.StoreListToLinesAppend(
                rootPath + @"\" + Environment.MachineName,
                new List<string>() { "I am alive #" + DateTime.Now.ToString() });
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Task.Run(() => new Experiments().nsichneuExperimentsB());
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            PathCounts.CountPath();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Task.Run(() => { new TestDataGeneration(
                rootPath+"testdata",
                null,
                Convert.ToInt32(textBox2.Text),
                null
            ).BestMoveRandomTestSet(); });
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Task.Run(()=>
            {
                int totalRuningTimes = Convert.ToInt32(textBox3.Text);
                for (int i = 0; i < totalRuningTimes; i++)
                {
                    rootPath = textBox4.Text;
                    rootPath = rootPath + i.ToString() + @"\";
                    if (Directory.Exists(rootPath))
                    {
                        Directory.Delete(rootPath,true);
                    }
                    Directory.CreateDirectory(rootPath);
                    new Experiments().BestMoveExperimentsB(rootPath, Convert.ToInt32(textBox2.Text));
                }
                Console.WriteLine("Finish");
            });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string selectedAlgorithm = checkedListBox2.SelectedItem.ToString();
            Task.Run(() =>
            {
                int totalRuningTimes = Convert.ToInt32(textBox3.Text);
                for (int i = 0; i < totalRuningTimes; i++)
                {
                    rootPath = textBox4.Text;
                    rootPath = rootPath + i.ToString() + @"\";
                    if (Directory.Exists(rootPath))
                    {
                        Directory.Delete(rootPath, true);
                    }
                    Directory.CreateDirectory(rootPath);
                    new Experiments2().BestMoveExperimentsB2(rootPath, Convert.ToInt32(textBox2.Text), selectedAlgorithm);
                }
                Console.WriteLine("Finish");
            });

        }

        private void button6_Click(object sender, EventArgs e)
        {
            Matrix<double> m = Matrix<double>.Build.Dense(5, 2);
            double[] weights = null;
            m[0, 0] = 0.7;
            m[0, 1] = 0;
            m[1, 0] = 0;
            m[1, 1] = 0.7;
            m[2, 0] = 0.3;
            m[2, 1] = 0.4;
            m[3, 0] = 0.8;
            m[3, 1] = 0.1;
            m[4, 0] = 0.2;
            m[4, 1] = 0.7;
            GoalProgramming.MinTrigProbCal(m, out weights);
        }
    }
}

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

namespace GADEApproach
{
    public partial class Form1 : Form
    {
        List<int> numOfLablesArray = null;
        List<double[]> entropyPropotionArray = null;
        string rootPath = null;
        public Form1()
        {
            InitializeComponent();
            numOfLablesArray = new List<int>();
            entropyPropotionArray = new List<double[]>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() => Experiments.ExperimentsA(numOfLablesArray.ToArray(),entropyPropotionArray.ToArray(), rootPath));
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
                    rootPath = fbd.SelectedPath;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Invalid Folder");
                }
            }
            label1.Text = rootPath;
        }
    }
}

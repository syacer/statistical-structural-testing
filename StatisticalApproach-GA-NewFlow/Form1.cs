using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StatisticalApproach.Framework;

namespace StatisticalApproach
{
    public partial class Form1 : Form
    {
        public IAppBuilder app;
        private Record record;
        private List<Form2> lForm2 = new List<Form2>();
        private Stopwatch globalWatch = new Stopwatch();
        public Form1()
        {
            InitializeComponent();
            app = new App();
            numericUpDown1.Maximum = 100;
        }
         
        private void button1_Click(object sender, EventArgs e)
        {
            int numOfRuns = (int)numericUpDown1.Value;
            chart1.Series.Remove(chart1.Series[0]);
            Random rnd = new Random();
            for (int i = 0; i < numOfRuns; i++)
            {
                chart1.Series.Add("Fitness_" + i.ToString());
                chart1.Series[i].ChartType = System.Windows.Forms.DataVisualization
                    .Charting.SeriesChartType.Point;
                chart1.Series[i].Color = Color.FromArgb(
                    rnd.Next(1, 255),
                    rnd.Next(1, 255),
                    rnd.Next(1, 255));
            }
            timer1.Start();
            globalWatch.Start();
            record = new Record(numOfRuns);
            EnvironmentVar[] enVars = new EnvironmentVar[numOfRuns];
            App frontApp = new App();
            frontApp.Use<SUTInitialization>((int)numericUpDown2.Value, record);
            frontApp.Use<TaskDistributor>(enVars, numOfRuns, record, Convert.ToInt16(textBox2.Text));
            frontApp.Use<Monitor>(record, enVars);
            frontApp.GetAppFunc().Invoke(null);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < record.updateIndicate.Count(); i++)
            {
                if (record.updateDisplay[i] == true)
                {
     
                    if (Convert.ToInt16(record.currentGen[i][0]) % 100 != 0)
                    {
                        record.updateDisplay[i] = false;
                        continue;
                    }
                    // Calculate Avg Fitness
                    for (int j = 0; j < record.currentFitnessList[i].Count; j++)
                    {
                        chart1.Series[i].Points.AddXY(record.currentFitnessList[i][j].Item1, record.currentFitnessList[i][j].Item2);
                    }
                    //lForm2[i].Show();
                    //lForm2[i].UpdateChart1Form2();
                    record.updateDisplay[i] = false;
                }
            }
            label3.Text = Math.Round(globalWatch.ElapsedMilliseconds*1.0/(1000*60),2).ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        }
    }
}

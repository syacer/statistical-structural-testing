using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StatisticalApproach_GA
{
    public partial class Form1 : Form
    {
        public IAppBuilder app;
        private Record record;
        public Form1()
        {
            InitializeComponent();
            app = new App();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int numOfRuns = (int)numericUpDown1.Value;
            string seriesName = "fitness/Generation";
            chart1.Series.Remove(chart1.Series[0]);
            Random rnd = new Random();
            for (int i = 0; i < numOfRuns; i++)
            {
                chart1.Series.Add(seriesName+"_"+i.ToString());
                chart1.Series[i].ChartType = System.Windows.Forms.DataVisualization
                    .Charting.SeriesChartType.Line;
                chart1.Series[i].Color = Color.FromArgb(
                    rnd.Next(1, 255), 
                    rnd.Next(1, 255), 
                    rnd.Next(1, 255));
            }

            timer1.Start();
            record = new Record(numOfRuns);
            EnvironmentVar[] enVars = new EnvironmentVar[numOfRuns];
            App frontApp = new App();
            frontApp.Use<SUTInitialization>(textBox1.Text, (int)numericUpDown2.Value,record);
            frontApp.Use<TaskDistributor>(enVars,numOfRuns,record,Convert.ToInt16(textBox2.Text));
            frontApp.Use<Monitor>(record,enVars);
            frontApp.GetAppFunc().Invoke(null);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string tempStr1 = null, tempStr2 = null;
            string tempStr3 = null, tempStr4 = null;
            for (int i = 0; i < record.currentGen.Count(); i++)
            {
                if (record.updateIndicate[i] == true)
                {
                    double fitness = (double)record.fitRecord.Rows[record.currentGen[i]][i + 1];
                    chart1.Series[i].Points.AddXY(record.currentGen[i],fitness);
                    record.updateIndicate[i] = false;
                }
                if (i <= record.currentGen.Count() / 2)
                {
                    tempStr1 = tempStr1 + " " + record.currentGen[i].ToString();
                    tempStr2 = tempStr2 + " " + Math.Round((double)record.fitRecord.Rows[record.currentGen[i]][i + 1], 3);
                }
                else
                {
                    tempStr3 = tempStr3 + " " + record.currentGen[i].ToString();
                    tempStr4 = tempStr4 + " " + Math.Round((double)record.fitRecord.Rows[record.currentGen[i]][i + 1], 3);
                }
            }
            Gen_label.Text = tempStr1;
            Fit_label.Text = tempStr2;
            Gen_Label2.Text = tempStr3;
            Fitness_Label2.Text = tempStr4;
            label3.Text = record.gaWatch.Elapsed.ToString();
            label8.Text = record.warmUpWatch.Elapsed.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StatisticalApproach.Framework;

namespace StatisticalApproach
{
    public partial class Form2 : Form
    {
        private int _displayRunNumber = -1;
        private Record _record;
        public Form2( int runNum, Record record)
        {
            InitializeComponent();
            _displayRunNumber = runNum;
            _record = record;

            chart1.Series.Remove(chart1.Series[0]);
            Random rnd = new Random();

            chart1.Series.Add("EstFitness/gen");
            chart1.Series.Add("TrueFitness/gen");
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization
                .Charting.SeriesChartType.Line;
            chart1.Series[0].Color = Color.FromArgb(
                rnd.Next(1, 255),
                rnd.Next(1, 255),
                rnd.Next(1, 255));
            chart1.Series[1].ChartType = System.Windows.Forms.DataVisualization
                .Charting.SeriesChartType.Line;
            chart1.Series[1].Color = Color.FromArgb(
                rnd.Next(1, 255),
                rnd.Next(1, 255),
                rnd.Next(1, 255));
        }

        //public void UpdateChart1Form2()
        //{

        //    if (_record.updateIndicate[_displayRunNumber] == true)
        //    {
        //        double fitness = ((List<double>)_record.fitRecord.Rows[_record.currentGen[_displayRunNumber]][_displayRunNumber + 1])[0];
        //        double truFitness = ((List<double>)_record.fitRecord.Rows[_record.currentGen[_displayRunNumber]][_displayRunNumber + 1])[1];
        //        chart1.Series[0].Points.AddXY(_record.currentGen[_displayRunNumber], fitness);
        //        chart1.Series[1].Points.AddXY(_record.currentGen[_displayRunNumber], truFitness);
        //        if (label1.Text != _record.bestSolution)
        //        {
        //            label1.Text = _record.bestSolution;
        //            label1.BackColor = Color.Red;
        //        }
        //        else
        //        {
        //            label1.BackColor = Color.Green;
        //        }
        //    }
        //}
    }
}

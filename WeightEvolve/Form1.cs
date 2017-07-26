using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeightEvolve
{
    public partial class Form1 : Form
    {

        public shareData data = new shareData() {
            fitness = 0,
            update = false,
            generation = 0
        };

        public Form1()
        {
            InitializeComponent();
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization
                .Charting.SeriesChartType.Line;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(()=> new DE(data).DE_Start());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task.Run(() => new DE(data).LabelPrepration());
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (data.update == true)
            {
                richTextBox2.AppendText(data.strbuf + Environment.NewLine);
                data.strbuf = null;
                chart1.Series[0].Points.AddXY(data.generation,data.fitness);
                data.update = false;
            }
        }
    }
}

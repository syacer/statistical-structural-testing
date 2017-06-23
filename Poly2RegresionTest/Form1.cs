using Accord.IO;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Linear;
using Components;
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
using ZedGraph;

namespace Poly2RegresionTest
{
    public partial class Form1 : Form
    {
        string[] columnNames;
        public Form1()
        {
            InitializeComponent();
            columnNames = new string[2];
            columnNames[0] = "x";
            columnNames[1] = "y";
        }


        public void PolyRegression()
        {

            if (dgvTestingSource.DataSource == null)
            {
                MessageBox.Show("Please Select a data set");
                return;
            }


            // Creates a matrix from the source data table
            double[,] table = (dgvLearningSource.DataSource as DataTable).ToMatrix();

            double[,] data = table;

            // Let's retrieve the input and output data:
            double[] inputs = data.GetColumn(0);  // X
            double[] outputs = data.GetColumn(1); // Y

            // We can create a learning algorithm
            var ls = new PolynomialLeastSquares()
            {
                Degree = 2
            };

            // Now, we can use the algorithm to learn a polynomial
            PolynomialRegression poly = ls.Learn(inputs, outputs);

            // The learned polynomial will be given by
            string str = poly.ToString("N1"); // "y(x) = 1.0x^2 + 0.0x^1 + 0.0"

            // Where its weights can be accessed using
            double[] weights = poly.Weights;   // { 1.0000000000000024, -1.2407665029287351E-13 }
            double intercept = poly.Intercept; // 1.5652369518855253E-12

            // Finally, we can use this polynomial
            // to predict values for the input data
            double[] pred = poly.Transform(inputs);

            // Where the mean-squared-error (MSE) should be
            double error = new SquareLoss(outputs).Loss(pred); // 0.0

            double[][] tmpInputs = new double[inputs.Length][];
            for (int i = 0; i < inputs.Length; i++)
            {
                tmpInputs[i] = new double[1]{ inputs[i] };
            }
            CreateResultScatterplot(zedGraphControl1, tmpInputs, outputs, pred);
        }
        public void CreateResultScatterplot(ZedGraphControl zgc, double[][] inputs, double[] expected, double[] output)
        {
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();

            // Set the titles
            myPane.Title.IsVisible = false;
            myPane.XAxis.Title.Text = columnNames[0];
            myPane.YAxis.Title.Text = columnNames[1];


            // Regression problem
            PointPairList list1 = new PointPairList(); // svm output
            PointPairList list2 = new PointPairList(); // expected output
            for (int i = 0; i < inputs.Length; i++)
            {
                list1.Add(inputs[i][0], output[i]);
                list2.Add(inputs[i][0], expected[i]);
            }

            // Add the curve
            LineItem myCurve = myPane.AddCurve("Model output", list1, Color.Blue, SymbolType.Diamond);
            myCurve.Line.IsVisible = true;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Blue);

            myCurve = myPane.AddCurve("Data output", list2, Color.Red, SymbolType.Diamond);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Red);

            // Fill the background of the chart rect and pane
            myPane.Fill = new Fill(Color.WhiteSmoke);

            zgc.AxisChange();
            zgc.Invalidate();
        }

        private void MenuFileOpen_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                string extension = Path.GetExtension(filename);
                if (extension == ".xls" || extension == ".xlsx")
                {
                    ExcelReader db = new ExcelReader(filename, true, false);
                    TableSelectDialog t = new TableSelectDialog(db.GetWorksheetList());

                    if (t.ShowDialog(this) == DialogResult.OK)
                    {
                        DataTable tableSource = db.GetWorksheet(t.Selection);
                        this.dgvLearningSource.DataSource = tableSource;
                        this.dgvTestingSource.DataSource = tableSource.Copy();
                    }
                }
            }
        }

        private void Learning_Click(object sender, EventArgs e)
        {
            PolyRegression();
        }
    }
}

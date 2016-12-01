using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace Lab_4
{
    public enum PointType
    {
        First,
        Second
    }

    public partial class Form1 : Form
    {
        const string PathToPoints = "../../Figure.txt";

        List<DataPoint> outerPoints;


        List<DataPoint> figurePoints = new List<DataPoint>();
        Calculation calc;
        public Form1()
        {
            InitializeComponent();
            InitChart();
            calc = new Calculation(this.figurePoints, this.chart1);
        }
        public List<DataPoint> OuterPoints
        {
            get
            {
                if(this.outerPoints == null)
                {
                    this.outerPoints = calc.GetDistinct();
                    outerPoints = calc.GetOuterPoints(outerPoints);
                    foreach (var point in this.figurePoints)
                        outerPoints.Add(point);
                }
                return outerPoints;
            }

            set
            {
                outerPoints = value;
            }
        }
        private void InitChart()
        {
            Series series = new Series("Figure");
            series.ChartType = SeriesChartType.Line;
            try
            {
                using (StreamReader reader = new StreamReader(PathToPoints))
                {
                    string temp = null;
                    while ((temp = reader.ReadLine()) != null)
                    {
                        if (temp == "")
                            continue;
                        string[] coord = temp.Split(';');
                        series.Points.Add(new DataPoint(Convert.ToDouble(coord[0]), Convert.ToDouble(coord[1])));
                        figurePoints.Add(new DataPoint(Convert.ToDouble(coord[0]), Convert.ToDouble(coord[1])));
                    }
                }
            }
            catch (Exception) { }
            this.chart1.Series.Add(series);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataPointCollection points = this.chart1.Series["Figure"].Points;
            points.RemoveAt(points.Count - 1);

            
            while (true)
            {
                if (points.Count <= 3)
                    break;

                DataPoint[] angleCoord = calc.GetMinAngle();
                DataPoint p = null;
                DataPoint p1 = null;

                p = calc.GetPoint(angleCoord, PointType.First);
                p1 = calc.GetPoint(angleCoord, PointType.Second);

                int index = points.IndexOf(angleCoord[1]);                
                points.RemoveAt(index);

                if (!calc.Contains(p1))
                    calc.InsertPoint(false, p1, index);

                if (angleCoord.Length == 3)
                {
                    if (!calc.Contains(p))
                        calc.InsertPoint(false, p, index);

                    CreateTriangle(p, new DataPoint(angleCoord[1].XValue, angleCoord[1].YValues[0]), p1);
                }
                else
                {
                    if (!calc.Contains(angleCoord[3]))
                        calc.InsertPoint(false, angleCoord[3], index);

                    if (!calc.Contains(p))
                        calc.InsertPoint(false, p, index);

                    CreateTriangle(p1, angleCoord[1], angleCoord[3]);
                    CreateTriangle(p, angleCoord[3], angleCoord[1]);
                }
            }
            this.chart1.Series["Figure"].Enabled = false;
             MessageBox.Show("Count of triangles: " + counter);

        }



        int counter;

      

        private void CreateTriangle(DataPoint p1, DataPoint p2, DataPoint p3)
        {
            Series triangle = new Series("Triangle" + counter++);
            triangle.ChartType = SeriesChartType.Line;
            triangle.Color = Color.Green;

            triangle.Points.Add(p1);
            triangle.Points.Add(p2);
            triangle.Points.Add(p3);
            triangle.Points.Add(p1);
            this.chart1.Series.Add(triangle);
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            this.label_Y.Location = new Point(e.X + 1, e.Y - this.label_Y.Height / 2 + 15);
            this.label_X.Location = new Point(e.X - this.label_X.Width / 2, e.Y + 15);

            this.label_Coord.Location = new Point(e.X + 10, e.Y - 20);
            try
            {
                double xVal = this.chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                double yVal = this.chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);

                this.label_Coord.Text = String.Format("x = {0:0.000} ; y = {1:0.000}", xVal, yVal);
            }
            catch (Exception) { return; }
        }
        private void button2_Click(object sender, EventArgs e)
        {                
            List<DataPoint> innerPoints = calc.GetInnerPoints(OuterPoints);
            
            for (int i = 0; i < innerPoints.Count; i++)
            {
                List<DataPoint> radiusPoints = GetRadiusPoints(innerPoints[i]);

                double sumX = 0;
                double sumY = 0;
                foreach (var pointA in radiusPoints)
                {
                    if (pointA.XValue == innerPoints[i].XValue && pointA.YValues[0] == pointA.YValues[0])
                        continue;
                    sumX += pointA.XValue;
                    sumY += pointA.YValues[0];
                }
                DataPoint newPoint = new DataPoint(sumX / (radiusPoints.Count ), sumY / (radiusPoints.Count ));
           
                for (int j = 0; j < this.chart1.Series.Count; j++)
                {
                    if (!this.chart1.Series[j].Name.Contains("Triangle"))
                        continue;

                    for (int k = 0; k < this.chart1.Series[j].Points.Count; k++)
                    {
                        if (this.chart1.Series[j].Points[k].XValue == innerPoints[i].XValue &&
                            this.chart1.Series[j].Points[k].YValues[0] == innerPoints[i].YValues[0])
                        {
                            this.chart1.Series[j].Points.RemoveAt(k);
                            this.chart1.Series[j].Points.Insert(k, newPoint);
                        }
                    }
                }
                int index = calc.allPoints.IndexOf(innerPoints[i]);
                calc.allPoints.RemoveAt(index);
                calc.allPoints.Insert(index, newPoint);
            }     
        }

        private List<DataPoint> GetRadiusPoints(DataPoint dataPoint)
        {
            List<DataPoint> result = new List<DataPoint>();

            for (int j = 0; j < this.chart1.Series.Count; j++)
            {
                if (!this.chart1.Series[j].Name.Contains("Triangle"))
                    continue;

                bool toAdd = false;
                for (int k = 0; k < this.chart1.Series[j].Points.Count; k++)
                {
                    if (this.chart1.Series[j].Points[k].XValue == dataPoint.XValue &&
                        this.chart1.Series[j].Points[k].YValues[0] == dataPoint.YValues[0])
                    {
                        toAdd = true;
                        break;
                    }
                }
                if (toAdd)
                {
                    for (int k = 0; k < this.chart1.Series[j].Points.Count; k++)
                    {
                        if (!result.Contains(this.chart1.Series[j].Points[k]) &&
                            this.chart1.Series[j].Points[k].XValue != dataPoint.XValue &&
                            this.chart1.Series[j].Points[k].YValues[0] != dataPoint.YValues[0])
                            result.Add(this.chart1.Series[j].Points[k]);
                    }
                }
            }

            return result;
        }
    }
}

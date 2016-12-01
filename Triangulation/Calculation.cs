using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Lab_4
{
    public class Calculation
    {
        double LenthOfParticle = 0.5;
        const double KoefD = 1.4;
        List<DataPoint> figurePoints;
        public List<DataPoint> allPoints = new List<DataPoint>();
        Chart chart1;
        public Calculation(List<DataPoint> figurePoints, Chart chart1)
        {
            this.figurePoints = figurePoints;
            this.chart1 = chart1;
        }
        public List<DataPoint> GetOuterPoints(List<DataPoint> points)
        {

            List<DataPoint> result = new List<DataPoint>();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < figurePoints.Count - 1; j++)
                {
                    double xDiv = 0;
                    double yDiv = 0;

                    if ((figurePoints[j + 1].XValue - figurePoints[j].XValue) == 0 && points[i].XValue == figurePoints[j].XValue)
                    {
                        result.Add(points[i]); continue;
                    }
                    if ((figurePoints[j + 1].YValues[0] - figurePoints[j].YValues[0]) == 0 && points[i].YValues[0] == figurePoints[j].YValues[0])
                    {
                        result.Add(points[i]); continue;
                    }

                    try
                    {
                        xDiv = (points[i].XValue - figurePoints[j].XValue) / (figurePoints[j + 1].XValue - figurePoints[j].XValue);
                        yDiv = (points[i].YValues[0] - figurePoints[j].YValues[0]) / (figurePoints[j + 1].YValues[0] - figurePoints[j].YValues[0]);
                    }
                    catch
                    {

                    }

                    double dif = xDiv - yDiv;
                    if (Math.Abs(xDiv - yDiv) < LenthOfParticle / 5)
                        result.Add(points[i]);
                }
            }
            return result;
        }
        public List<DataPoint> GetDistinct()
        {
            List<DataPoint> result = new List<DataPoint>();
            for (int i = 0; i < allPoints.Count; i++)
            {
                bool toAdd = true;
                for (int j = 0; j < result.Count; j++)
                {
                    if (result[j].XValue == allPoints[i].XValue &&
                        result[j].YValues[0] == allPoints[i].YValues[0])
                    {
                        toAdd = false;
                        break;
                    }
                }
                if (toAdd)
                    result.Add(allPoints[i]);
            }
            return result;
        }
        public void InsertPoint(bool isOnTheEdge, DataPoint point, int index)
        {
            allPoints.Add(point);
            this.chart1.Series["Figure"].Points.Insert(index, point);
        }
        public DataPoint GetPoint(DataPoint[] angleCoord, PointType type)
        {
            double xCoord = 0;
            double yCoord = 0;
            double lenth = 0;

            switch (type)
            {
                case PointType.First:
                    lenth = Math.Sqrt(Math.Pow(angleCoord[2].XValue - angleCoord[1].XValue, 2) +
                    Math.Pow(angleCoord[2].YValues[0] - angleCoord[1].YValues[0], 2));

                    xCoord = angleCoord[1].XValue + LenthOfParticle / lenth * (angleCoord[2].XValue - angleCoord[1].XValue);
                    yCoord = angleCoord[1].YValues[0] + LenthOfParticle / lenth * (angleCoord[2].YValues[0] - angleCoord[1].YValues[0]);

                    DataPoint point = CheckRadius(xCoord, yCoord);
                    if (point != null)
                    {
                        xCoord = point.XValue;
                        yCoord = point.YValues[0];
                    }
                    if (lenth < LenthOfParticle)
                    {
                        xCoord = angleCoord[2].XValue;
                        yCoord = angleCoord[2].YValues[0];

                    }
                    break;

                case PointType.Second:
                    lenth = Math.Sqrt(Math.Pow(angleCoord[1].XValue - angleCoord[0].XValue, 2) +
                    Math.Pow(angleCoord[1].YValues[0] - angleCoord[0].YValues[0], 2));

                    xCoord = angleCoord[1].XValue - LenthOfParticle / lenth * (angleCoord[1].XValue - angleCoord[0].XValue);
                    yCoord = angleCoord[1].YValues[0] - LenthOfParticle / lenth * (angleCoord[1].YValues[0] - angleCoord[0].YValues[0]);

                    DataPoint point1 = CheckRadius(xCoord, yCoord);
                    if (point1 != null)
                    {
                        xCoord = point1.XValue;
                        yCoord = point1.YValues[0];
                    }
                    if (lenth < LenthOfParticle)
                    {
                        xCoord = angleCoord[0].XValue;
                        yCoord = angleCoord[0].YValues[0];

                    }
                    break;
            }
            return new DataPoint((float)xCoord, (float)yCoord);
        }

        internal List<DataPoint> GetInnerPoints(List<DataPoint> outerPoints)
        {
            List<DataPoint> result = new List<DataPoint>();
            for (int i = 0; i < this.allPoints.Count; i++)
            {
                bool toAdd = true;
                for (int j = 0; j < outerPoints.Count; j++)
                {
                    if (allPoints[i].XValue == outerPoints[j].XValue &&
                        allPoints[i].YValues[0] == outerPoints[j].YValues[0])
                    {
                        toAdd = false;
                        break;
                    }
                }
                if (toAdd)
                    result.Add(allPoints[i]);
            }

            return result;
        }

        public DataPoint[] GetMinAngle()
        {
            DataPointCollection points = this.chart1.Series["Figure"].Points;

            double angle = GetAngle(points[0].XValue - points[points.Count - 1].XValue, points[0].YValues[0] - points[points.Count - 1].YValues[0],
                    points[points.Count - 2].XValue - points[points.Count - 1].XValue, points[points.Count - 2].YValues[0] - points[points.Count - 1].YValues[0]);
            List<DataPoint> angleCoord = new List<DataPoint> { points[0], points[points.Count - 1], points[points.Count - 2] }; ;

            for (int i = points.Count - 1; i > 0; i--)
            {
                double tempAngle;

                if (i == 1)
                    tempAngle = GetAngle(points[i].XValue - points[i - 1].XValue, points[i].YValues[0] - points[i - 1].YValues[0],
                    points[points.Count - 1].XValue - points[i - 1].XValue, points[points.Count - 1].YValues[0] - points[i - 1].YValues[0]);
                else
                    tempAngle = GetAngle(points[i].XValue - points[i - 1].XValue, points[i].YValues[0] - points[i - 1].YValues[0],
                    points[i - 2].XValue - points[i - 1].XValue, points[i - 2].YValues[0] - points[i - 1].YValues[0]);

                if (tempAngle < angle)
                {
                    if (i == 1)
                    {
                        if (IsRightThree(points[points.Count - 1].XValue - points[i - 1].XValue, points[points.Count - 1].YValues[0] - points[i - 1].YValues[0],
                            points[i - 1].XValue - points[i].XValue, points[i - 1].YValues[0] - points[i].YValues[0]))
                        {
                            angleCoord = new List<DataPoint> { points[i], points[i - 1], points[points.Count - 1] };
                            angle = tempAngle;
                        }
                    }
                    else
                    {
                        if (IsRightThree(points[i - 2].XValue - points[i - 1].XValue, points[i - 2].YValues[0] - points[i - 1].YValues[0],
                            points[i - 1].XValue - points[i].XValue, points[i - 1].YValues[0] - points[i].YValues[0]))
                        {
                            angleCoord = new List<DataPoint> { points[i], points[i - 1], points[i - 2] };
                            angle = tempAngle;
                        }
                    }

                }
            }
            if (angle > 75)
                angleCoord = SplitIntoTwo(angleCoord);

            return angleCoord.ToArray();
        }

        public List<DataPoint> SplitIntoTwo(List<DataPoint> angleCoord)
        {
            double length1 = Math.Sqrt(Math.Pow(angleCoord[1].XValue - angleCoord[0].XValue, 2) + Math.Pow(angleCoord[1].YValues[0] - angleCoord[0].YValues[0], 2));
            double length2 = Math.Sqrt(Math.Pow(angleCoord[2].XValue - angleCoord[1].XValue, 2) + Math.Pow(angleCoord[2].YValues[0] - angleCoord[1].YValues[0], 2));
            double length3 = Math.Sqrt(Math.Pow(angleCoord[2].XValue - angleCoord[0].XValue, 2) + Math.Pow(angleCoord[2].YValues[0] - angleCoord[0].YValues[0], 2));

            double rel = length1 / length2;
            double DB = length3 / (1 + rel);

            double tempXCoord = angleCoord[2].XValue - DB / length3 * (angleCoord[2].XValue - angleCoord[0].XValue);
            double tempYCoord = angleCoord[2].YValues[0] - DB / length3 * (angleCoord[2].YValues[0] - angleCoord[0].YValues[0]);

            double lengthCD = Math.Sqrt(Math.Pow(tempXCoord - angleCoord[1].XValue, 2) + Math.Pow(tempYCoord - angleCoord[1].YValues[0], 2));
            if (lengthCD <= LenthOfParticle)
            {
                double XCoord = angleCoord[1].XValue - (angleCoord[1].XValue - tempXCoord) * (LenthOfParticle * KoefD / lengthCD);
                double YCoord = angleCoord[1].YValues[0] + (tempYCoord - angleCoord[1].YValues[0]) * (LenthOfParticle * KoefD / lengthCD);

                DataPoint point = CheckRadius(XCoord, YCoord);
                if (point != null)
                {
                    XCoord = point.XValue;
                    YCoord = point.YValues[0];
                }
                if (IsInsideFigure(XCoord, YCoord))
                    angleCoord.Add(new DataPoint((float)XCoord, (float)YCoord));
                else
                    angleCoord.Add(new DataPoint((float)tempXCoord, (float)tempYCoord));
                double l = Math.Sqrt(Math.Pow(XCoord - angleCoord[1].XValue, 2) + Math.Pow(YCoord - angleCoord[1].YValues[0], 2));
            }
            else
            {
                double XCoord = angleCoord[1].XValue - (angleCoord[1].XValue - tempXCoord) / (lengthCD / (LenthOfParticle * KoefD));
                double YCoord = angleCoord[1].YValues[0] + (tempYCoord - angleCoord[1].YValues[0]) / (lengthCD / (LenthOfParticle * KoefD));

                angleCoord.Add(new DataPoint((float)XCoord, (float)YCoord));
            }
            return angleCoord;
        }

        public bool IsInsideFigure(double xCoord, double yCoord)
        {
            DataPointCollection points = this.chart1.Series["Figure"].Points;
            double minXVal = points.FindMinByValue("X").XValue - 1;
            int count = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (CrossVectors(
                    new List<PointF>() { new PointF((float)minXVal, (float)yCoord), new PointF((float)xCoord, (float)yCoord) },
                    new List<PointF>(){ new PointF((float)points[i].XValue, (float)points[i].YValues[0]),
                        new PointF((float)points[i+1].XValue, (float)points[i+1].YValues[0]) }))
                    count++;
            }
            if (CrossVectors(
                    new List<PointF>() { new PointF((float)minXVal, (float)yCoord), new PointF((float)xCoord, (float)yCoord) },
                    new List<PointF>(){ new PointF((float)points[points.Count-1].XValue, (float)points[points.Count-1].YValues[0]),
                        new PointF((float)points[0].XValue, (float)points[0].YValues[0]) }))
                count++;
            if (count % 2 == 0)
                return false;
            return true;
        }
        public bool CrossVectors(List<PointF> line1, List<PointF> line2)
        {
            //чи перетинаються
            double[] vectorMult = new double[4];
            vectorMult[0] = (line1[1].X - line1[0].X) * (line2[1].Y - line1[0].Y) - (line1[1].Y - line1[0].Y) * (line2[1].X - line1[0].X);
            vectorMult[1] = (line1[1].X - line1[0].X) * (line2[0].Y - line1[0].Y) - (line1[1].Y - line1[0].Y) * (line2[0].X - line1[0].X);
            vectorMult[2] = (line2[1].X - line2[0].X) * (line1[1].Y - line2[0].Y) - (line2[1].Y - line2[0].Y) * (line1[1].X - line2[0].X);
            vectorMult[3] = (line2[1].X - line2[0].X) * (line1[0].Y - line2[0].Y) - (line2[1].Y - line2[0].Y) * (line1[0].X - line2[0].X);

            if (vectorMult[0] * vectorMult[1] < 0 && vectorMult[2] * vectorMult[3] < 0)
                return true;
            return false;
        }
        public DataPoint CheckRadius(double xCoord, double yCoord)
        {
            DataPointCollection points = this.chart1.Series["Figure"].Points;
            DataPoint point = null;
            for (int i = 0; i < points.Count; i++)
            {
                if (Math.Abs(points[i].XValue - xCoord) < (LenthOfParticle / 3) &&
                    Math.Abs(points[i].YValues[0] - yCoord) < (LenthOfParticle / 3))
                    point = points[i];
            }
            return point;
        }


        public bool Contains(DataPoint point)
        {
            bool result = false;
            for (int i = 0; i < this.chart1.Series["Figure"].Points.Count; i++)
            {
                if (this.chart1.Series["Figure"].Points[i].XValue == point.XValue &&
                    this.chart1.Series["Figure"].Points[i].YValues[0] == point.YValues[0])
                {
                    result = true;
                    break;
                }
            }
            return result;
        }






        public bool IsRightThree(double line1X, double line1Y, double line2X, double line2Y)
        {
            double res = line1X * line2Y - line1Y * line2X;
            if (res < 0)
                return true;
            return false;
        }


        public double GetAngle(double line1X, double line1Y, double line2X, double line2Y)
        {
            double angle = 0;
            try
            {
                double line1Length = Math.Sqrt(Math.Pow(line1X, 2) + Math.Pow(line1Y, 2));
                double line2Length = Math.Sqrt(Math.Pow(line2X, 2) + Math.Pow(line2Y, 2));

                angle = Math.Acos((line1X * line2X + line1Y * line2Y) / (line1Length * line2Length)) * 180 / Math.PI;
            }
            catch (Exception) { }

            return angle;
        }
    }
}

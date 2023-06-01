using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Numerics;
using System.Globalization;
using Prism.Commands;
using Prism.Mvvm;
using OpenCvSharp;

namespace SD2CircleTool.ViewModels
{
    public class CircleToolViewModel : BindableBase
    {
        private int _enemies = 4; public int Enemies { get { return _enemies; } set { SetProperty(ref _enemies, value); } }
        private double _speed = 5; public double Speed { get { return _speed; } set { SetProperty(ref _speed, value); } }
        private double _speed2 = 5; public double Speed2 { get { return _speed2; } set { SetProperty(ref _speed2, value); } }
        private bool _wave = false; public bool Wave { get { return _wave; } set { SetProperty(ref _wave, value); updateCircle2(X2, Y2, D2); } }
        private bool _isCW = true; public bool IsCW { get { return _isCW; } set { SetProperty(ref _isCW, value); } }
        private bool _isCW2 = true; public bool IsCW2 { get { return _isCW2; } set { SetProperty(ref _isCW2, value); } }
        private int _rows = 1; public int Rows { get { return _rows; } set { SetProperty(ref _rows, value); if (value > 1) { Wave = true; } else { Wave = false; } } }
        private double _x = 0; public double X { get { return _x; } set { SetProperty(ref _x, value); updateCircle(value, Y, D); } }
        private double _y = 0; public double Y { get { return _y; } set { SetProperty(ref _y, value); updateCircle(X, value, D); } }
        private double _d = 0.1; public double D { get { return _d; } set { if (value >= 0) { SetProperty(ref _d, value); updateCircle(X, Y, value); } } }
        private double _x2 = 0; public double X2 { get { return _x2; } set { SetProperty(ref _x2, value); updateCircle2(value, Y2, D2); } }
        private double _y2 = 0; public double Y2 { get { return _y2; } set { SetProperty(ref _y2, value); updateCircle2(X2, value, D2); } }
        private double _d2 = 0.1; public double D2 { get { return _d2; } set { if (value >= 0) { SetProperty(ref _d2, value); updateCircle2(X2, Y2, value); } } }
        private double _cirX = 0; public double CirX { get { return _cirX; } set { SetProperty(ref _cirX, value); } }
        private double _cirY = 0; public double CirY { get { return _cirY; } set { SetProperty(ref _cirY, value); } }
        private double _cirD = 0; public double CirD { get { return _cirD; } set { SetProperty(ref _cirD, value); } }
        private double _cirX2 = 0; public double CirX2 { get { return _cirX2; } set { SetProperty(ref _cirX2, value); } }
        private double _cirY2 = 0; public double CirY2 { get { return _cirY2; } set { SetProperty(ref _cirY2, value); } }
        private double _cirD2 = 0; public double CirD2 { get { return _cirD2; } set { SetProperty(ref _cirD2, value); } }
        private double _time = 5; public double Time { get { return _time; } set { SetProperty(ref _time, value); } }
        private int _id = 10; public int ID { get { return _id; } set { SetProperty(ref _id, value); } }

        private void updateCircle(double x, double y, double d)
        {
            CirX = 275 + (250 * (x - (d / 2)));
            CirY = 275 + (250 * (-y - (d / 2)));
            CirD = 250 * d;
            if (CirD <= 5) { CirD = 5; }
        }

        private void updateCircle2(double x, double y, double d)
        {
            CirX2 = 275 + (250 * (x - (d / 2)));
            CirY2 = 275 + (250 * (-y - (d / 2)));
            CirD2 = 250 * d;
            if (CirD2 <= 5) { CirD2 = 5; }
        }

        private double findAngleBetween(Vector3 a, Vector3 b)
        {
            double angle = Math.Acos(Vector3.Dot(a, b) / (a.Length() * b.Length()));
            Vector3 zVector = Vector3.Cross(a, b);
            if (zVector.Z > 0) { angle *= -1; } // CCW is negative
            return angle;
        }

        public DelegateCommand MakeCircle => new(() =>
        {

            float circleX = (float)X;
            float circleY = (float)Y;
            float circleX2 = (float)X2;
            float circleY2 = (float)Y2;

            string[] markers = new string[Enemies];

            for (int i = 0; i < Enemies; i++)
            {
                // Determines whether enemy shoots or not
                int amount = 0, amount2 = 0;

                // final vars
                double finalSpeed = 0, finalSpeed2 = 0;
                double finalAngle = 0, finalAngle2 = 0;

                // Set enemy position. use float because Vector2 doesn't like double
                double enemyAngle = (2 * Math.PI * (double)i) / (double)Enemies;
                float enemyX = (float)Math.Sin(enemyAngle);
                float enemyY = (float)Math.Cos(enemyAngle);
                Vector3 enemyVector = new Vector3(-enemyX, -enemyY, 0);

                Vector3 enemyToCircleVector = new Vector3(circleX - enemyX, circleY - enemyY, 0);

                double phi = findAngleBetween(enemyVector, enemyToCircleVector);

                Vector3.Cross(enemyVector, enemyToCircleVector);

                if ((D / 2) <= enemyToCircleVector.Length())
                {
                    double theta = Math.Asin((D / 2) / enemyToCircleVector.Length());
                    theta = Math.Abs(theta);
                    double l = Math.Sqrt(Math.Pow(enemyToCircleVector.Length(), 2) - Math.Pow((D / 2), 2));
                    finalSpeed = Speed * l;
                    if (IsCW) { finalAngle = phi - theta; }
                    else { finalAngle = phi + theta; }
                    finalAngle = finalAngle * 180 / Math.PI;
                    amount = 1;
                }

                if (Wave)
                {
                    enemyToCircleVector = new Vector3(circleX2 - enemyX, circleY2 - enemyY, 0);

                    phi = findAngleBetween(enemyVector, enemyToCircleVector);

                    if ((D2 / 2) <= enemyToCircleVector.Length())
                    {
                        double theta = Math.Asin((D2 / 2) / enemyToCircleVector.Length());
                        theta = Math.Abs(theta);
                        double l = Math.Sqrt(Math.Pow(enemyToCircleVector.Length(), 2) - Math.Pow((D2 / 2), 2));
                        finalSpeed2 = Speed2 * l;
                        if (IsCW2) { finalAngle2 = phi - theta; }
                        else { finalAngle2 = phi + theta; }
                        finalAngle2 = finalAngle2 * 180 / Math.PI;
                        amount2 = 1;
                    }
                }

                markers[i] = String.Empty;

                int LocalID = (int)((i + 0.2) / 10) + 1;

                if (i % 10 == 0)
                {
                    markers[i] += String.Format("  <PrefabMaster ID=\"{0}\" dur=\"0.5\">\r\n",
                        (int)((ID * 10) + LocalID));
                }

                markers[i] += String.Format(new CultureInfo("en-US"), "    <Bullet time=\"0\" en=\"{0}\" patt=\"normal\" type=\"arrow\" aim=\"center\" rows=\"{1}\" offset=\"{2:N3},{3:0.000}\" amt=\"{4},{5}\" speed=\"{6:0.000},{7:0.000}\" spread=\"0,0\" lyr=\"{8}\" />",
                    i + 1, Rows, finalAngle, finalAngle2, amount, amount2, finalSpeed, finalSpeed2, (i % 10) + 1);

                if (i % 10 == 9 || i == Enemies - 1)
                {
                    markers[i] += String.Format(new CultureInfo("en-US"), "\r\n  </PrefabMaster>\r\n  <Prefab ID=\"{0}\" time=\"{1:0.000}\" offset=\"0\" totalDur=\"0.3\" lyr=\"{2}\" />",
                        (int)((ID * 10) + LocalID), Time, LocalID);
                }
            }

            Clipboard.SetText(String.Join("\r\n", markers));
            MessageBox.Show("Copied: \r\n \r\n" + String.Join("\r\n", markers));
        });

        public DelegateCommand TestCommand => new(() =>
        {
            Point2f[] src = new Point2f[3];
            Point2f[] dst = new Point2f[3];
            src[2] = new Point2f(0, 0);
            src[0] = new Point2f(1, 0);
            src[1] = new Point2f(0, 1);
            dst[2] = new Point2f(2, 0); // Translate x+2
            dst[0] = new Point2f(2, 1);
            dst[1] = new Point2f(1, 0);
            Mat P = Cv2.GetAffineTransform(src, dst); // found: the translate value is always of 0, 0
            Mat P2 = Cv2.EstimateAffinePartial2D(InputArray.Create<Point2f>(src), InputArray.Create<Point2f>(dst));
            double[][] PArr= debugMat<double>(P2);
            int test = 0;
        });

        T[][] debugMat<T>(Mat mat) where T : unmanaged
        {
            int counter = 0;
            T[][] result = new T[mat.Rows][];

            if (mat.Rows > 0 && mat.Cols > 0)
            {
                for (int i = 0; i < mat.Rows; i++)
                {
                    result[i] = new T[mat.Cols];
                    for (int j = 0; j < mat.Cols; j++)
                    {
                        var stuff = mat.At<T>(i, j);
                        result[i][j] = stuff;

                        counter++;
                    }
                }
            }

            return result;
        }

        public CircleToolViewModel()
        {
            updateCircle(X, Y, D);
        }
    }
}

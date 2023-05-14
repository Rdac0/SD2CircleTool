using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using Prism.Commands;
using Prism.Mvvm;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;

namespace SD2CircleTool.ViewModels
{
    public class Onimai2TextViewModel : BindableBase
    {
        private double s2unitRatio = 800d / 448d;

        private Mat image;
        private Mat resizeImage;
        private double imageRatio { get { return (double)image.Cols / (double)image.Rows; } }
        private bool keepRatio; public bool KeepRatio { get { return keepRatio; } set { SetProperty(ref keepRatio, value); } }
        private AlignmentInfo alignment; public AlignmentInfo Alignment { get { return alignment; } set { SetProperty(ref alignment, value); UpdateRectPosition(); } }
        private ObservableCollection<AlignmentInfo> alignments; public ObservableCollection<AlignmentInfo> Alignments { get { return alignments; } set { SetProperty(ref alignments, value); } }

        #region Real Image Values
        private double x; public double X { get { return x; } set { SetProperty(ref x, value); UpdateRectPosition(); } }
        private double y; public double Y { get { return y; } set { SetProperty(ref y, value); UpdateRectPosition(); } }
        private double angle; public double Angle { get { return angle; } set { SetProperty(ref angle, value); UpdateRectPosition(); } }
        private double w; public double W { get { return w; } set { SetProperty(ref w, value); if (u) { u = false; H = (double)value / resRatio; u = true; } UpdateRectPosition(); } }
        private double h; public double H { get { return h; } set { SetProperty(ref h, value); if (u) { u = false; W = (double)value * resRatio; u = true; } UpdateRectPosition(); } }
        private bool u = true;

        private double ogXRes; public double OgXRes { get { return ogXRes; } set { SetProperty(ref ogXRes, value); UpdateRectPosition(); } }
        private double ogYRes; public double OgYRes { get { return ogYRes; } set { SetProperty(ref ogYRes, value); UpdateRectPosition(); } }

        #endregion

        #region Displayed Image Values
        private double dx; public double dX { get { return dx; } set { SetProperty(ref dx, value); } }
        private double dy; public double dY { get { return dy; } set { SetProperty(ref dy, value); } }
        private double dw; public double dW { get { return dw; } set { SetProperty(ref dw, value); } }
        private double dh; public double dH { get { return dh; } set { SetProperty(ref dh, value); } }

        #endregion


        private int xRes; public int XRes { get { return xRes; } set { SetProperty(ref xRes, Math.Clamp(value, 1, 9999)); if (uRes) { uRes = false; if (KeepRatio) { YRes = (int)((double)value / imageRatio); } UpdateFGImage(); uRes = true; } } }
        private int yRes; public int YRes { get { return yRes; } set { SetProperty(ref yRes, Math.Clamp(value, 1, 9999)); if (uRes) { uRes = false; if (KeepRatio) { XRes = (int)((double)value * imageRatio); } UpdateFGImage(); uRes = true; } } }
        private bool uRes = true;
        private double resRatio { get { return (double)xRes / (double)yRes; } }

        private double time; public double Time { get { return time; } set { SetProperty(ref time, value); } }
        private double duration; public double Duration { get { return duration; } set { SetProperty(ref duration, value); } }
        private double fadein; public double FadeIn { get { return fadein; } set { SetProperty(ref fadein, value); } }
        private double fadeout; public double FadeOut { get { return fadeout; } set { SetProperty(ref fadeout, value); } }

        private BitmapSource bgImage;  public BitmapSource BGImage { get { return bgImage; } }
        private BitmapSource fgImage;  public BitmapSource FGImage { get { return fgImage; } set { SetProperty(ref fgImage, value); } }

        private String _filePath; public String FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        private bool _isPathValid = false;
        private string imageText = "";

        private void UpdateFGImage(string imgPath)
        {
            image = new Mat(imgPath).CvtColor(ColorConversionCodes.BGR2BGRA);

            XRes = image.Cols;// calling this updates the image
            KeepRatio = false;
            YRes = image.Rows;
            KeepRatio = true; 

            OgXRes = image.Cols;
            OgYRes = image.Rows;

            if (imageRatio >= 16d / 9d) { W = 448; }
            else { H = 448d * 9d / 16d; }
        }

        private void UpdateFGImage()
        {
            Cv2.Resize(image, resizeImage, new OpenCvSharp.Size(xRes, yRes));
            FGImage = BitmapSourceConverter.ToBitmapSource(resizeImage);
            double tW = W;
            W = 1;
            W = tW;
        }

        // Turn real coords to display coords
        private void UpdateRectPosition()
        {
            dW = W * s2unitRatio; 
            dH = H * s2unitRatio;

            double tx = 224;
            double ty = 126;

            switch (Alignment)
            {
                case AlignmentInfo.Left:
                    tx += X;
                    tx += H * 0.5d * Math.Sin(Angle * Math.PI / 180d);

                    dX = tx * s2unitRatio;

                    ty -= Y;
                    ty -= H * 0.5d * Math.Cos(Angle * Math.PI / 180d);

                    dY = ty * s2unitRatio;
                    break;
                case AlignmentInfo.Right:
                    tx += X;
                    tx -= W * Math.Cos(Angle * Math.PI / 180d);
                    tx += H * 0.5d * Math.Sin(Angle * Math.PI / 180d);

                    dX = tx * s2unitRatio;

                    ty -= Y;
                    ty -= H * 0.5d * Math.Cos(Angle * Math.PI / 180d);
                    ty -= W * Math.Sin(Angle * Math.PI / 180d);

                    dY = ty * s2unitRatio;
                    break;
                case AlignmentInfo.Center:
                default:
                    tx += X;
                    tx -= W * 0.5d * Math.Cos(Angle * Math.PI / 180d);
                    tx += H * 0.5d * Math.Sin(Angle * Math.PI / 180d);

                    dX = tx * s2unitRatio;

                    ty -= Y;
                    ty -= H * 0.5d * Math.Cos(Angle * Math.PI / 180d);
                    ty -= W * 0.5d * Math.Sin(Angle * Math.PI / 180d);

                    dY = ty * s2unitRatio;
                    break;

            }
        }

        public DelegateCommand OpenFileCommand => new(() =>
        {
            _isPathValid = false;
            OpenFileDialog ofd = new()
            {
                Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg"
            };

            if (ofd.ShowDialog() == true)
            {
                FilePath = ofd.FileName;
                _isPathValid = true;
                UpdateFGImage(FilePath);
            }
        });

        public DelegateCommand ResetResCommand => new(() =>
        {
            KeepRatio = false;
            XRes = image.Cols;
            YRes = image.Rows;
            KeepRatio = true;
        });

        public DelegateCommand CompromiseResCommand => new(() =>
        {
            KeepRatio = true;
            if (imageRatio > 16d / 9d) { XRes = 448; }
            else { YRes = 252; }
        });


        public DelegateCommand CenterCommand => new(() =>
        {
            X = 0;
            Y = 0;
            switch (Alignment)
            {
                case AlignmentInfo.Left:
                    X -= W * 0.5d;
                    break;
                case AlignmentInfo.Right:
                    X += W * 0.5d;
                    break;
                case AlignmentInfo.Center:
                default:
                    break;
            }
        });

        public DelegateCommand FitCommand => new(() =>
        {
            if (imageRatio >= 16d / 9d) { W = 448; }
            else { H = 448d * 9d / 16d; }
            CenterCommand.Execute();
        });

        public DelegateCommand FillCommand => new(() =>
        {
            if (imageRatio <= 16d / 9d) { W = 448; }
            else { H = 448d * 9d / 16d; }
            CenterCommand.Execute();
        });

        public unsafe DelegateCommand SaveCommand => new(() =>
        {
            if (xRes * yRes >= 230400) // 360p image widescreen
            {
                if (MessageBox.Show("Warning, a text object this large may severely lag, and/or even crash your game! \n Continue?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
            }

            imageText = "";

            // Beginning Text Meta
            imageText += String.Format(new CultureInfo("en-US"), "  <Text time=\"{0:N3}\" txt=\"&lt;cspace=-.16em&gt;&lt;line-height=52%&gt;", Time);

            // Actual Text
            string[] imageTextLines = new string[yRes];

            Parallel.For(0, yRes, i =>
            {
                imageTextLines[i] = "";

                switch (Alignment)
                {
                    case AlignmentInfo.Left:
                        imageTextLines[i] += @"&lt;space=-0.06em&gt;";
                        break;
                    case AlignmentInfo.Right:
                        break;
                    case AlignmentInfo.Center:
                    default:
                        imageTextLines[i] += @"&lt;space=-0.32em&gt;";
                        break;

                }

                Vec4b* rowPtr = (Vec4b*)resizeImage.Ptr(i);

                string prevColStr = "";

                for (int j = 0; j< xRes; j++)
                {
                    string currColStr = "";

                    currColStr += BitConverter.ToString(new byte[] { rowPtr[j][2] });
                    currColStr += BitConverter.ToString(new byte[] { rowPtr[j][1] });
                    currColStr += BitConverter.ToString(new byte[] { rowPtr[j][0] });
                    if (rowPtr[j][3] < 255)
                    {
                        currColStr += BitConverter.ToString(new byte[] { rowPtr[j][3] });
                    }

                    if (currColStr != prevColStr)
                    {
                        imageTextLines[i] += @"&lt;color=#";
                        imageTextLines[i] += currColStr;
                        imageTextLines[i] += @"&gt;■";

                        prevColStr = currColStr;
                    }
                    else
                    {
                        imageTextLines[i] += @"■";
                    }
                }

                imageTextLines[i] += @"\n";
            });

            for(int i =0; i < yRes; i++)
            {
                imageText += imageTextLines[i];
            }

            double size = (W / 448d) * (332 / (double)XRes);
            string just = "";
            switch (Alignment)
            {
                case AlignmentInfo.Left:
                    just = "l";
                    break;
                case AlignmentInfo.Right:
                    just = "r";
                    break;
                case AlignmentInfo.Center:
                default:
                    just = "c";
                    break;

            }

            imageText += String.Format(new CultureInfo("en-US"), "\" size=\"{0:N3},{0:N3}\" dur=\"{1:N3}\" pos=\"{2:N3},{3:N3}\" fade=\"{4:N3},{5:N3}\" just=\"{6}\" col=\"FFFFFFFF\" font=\"noto thin none\" purp=\"always_on\" angle=\"{7:N3}\" lyr=\"10\" />",
                size, duration, X, Y, FadeIn, FadeOut, just, Angle);

            File.WriteAllText(FilePath + ".txt", imageText);

            MessageBox.Show("Save Complete!");

            Process.Start("explorer.exe", Path.GetDirectoryName(FilePath));

        });

        public Onimai2TextViewModel()
        {
            Mat bg = new Mat(@".\Assets\catMahiro.png");
            bgImage = BitmapSourceConverter.ToBitmapSource(bg);
            resizeImage = new Mat();

            KeepRatio = true;

            X = 0;
            Y = 0;
            Angle = 0;

            Alignments = new ObservableCollection<AlignmentInfo>()
            {
                AlignmentInfo.Left,
                AlignmentInfo.Center,
                AlignmentInfo.Right
            };

            Alignment = AlignmentInfo.Center;

            Time = 0;
            Duration = 1;
            FadeIn = 0;
            FadeOut = 0;
        }
    }

    public enum AlignmentInfo
    {
        Left, Center, Right
    }
}

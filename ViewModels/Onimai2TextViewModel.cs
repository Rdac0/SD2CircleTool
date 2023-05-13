using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Collections.ObjectModel;
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



        private BitmapSource bgImage;  public BitmapSource BGImage { get { return bgImage; } }
        private BitmapSource fgImage;  public BitmapSource FGImage { get { return fgImage; } set { SetProperty(ref fgImage, value); } }

        private String _filePath; public String FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        private bool _isPathValid = false;

        private void UpdateFGImage(string imgPath)
        {
            image = new Mat(imgPath); 
            XRes = image.Cols; // calling this updates the image
            if (imageRatio >= 16d / 9d) { W = 448; }
            else { H = 448d * 9d / 16d; }
        }

        private void UpdateFGImage()
        {
            Mat resizeMat = new Mat();
            Cv2.Resize(image, resizeMat, new OpenCvSharp.Size(xRes, yRes));
            FGImage = BitmapSourceConverter.ToBitmapSource(resizeMat);
            double tempw = W;
            W = 1;
            W = tempw;
        }

        // Turn real coords to display coords
        private void UpdateRectPosition()
        {
            dW = W * s2unitRatio; 
            dH = h * s2unitRatio;

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

        public Onimai2TextViewModel()
        {
            Mat bg = new Mat(@".\Assets\catMahiro.png");
            bgImage = BitmapSourceConverter.ToBitmapSource(bg);

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
        }
    }

    public enum AlignmentInfo
    {
        Left, Center, Right
    }
}

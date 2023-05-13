using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using Prism.Commands;
using Prism.Mvvm;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;

namespace SD2CircleTool.ViewModels
{
    public class Onimai2TextViewModel : BindableBase
    {
        private double x = 0; public double X { get { return x; } set { SetProperty(ref x, value); } }
        private double y = 0; public double Y { get { return y; } set { SetProperty(ref y, value); } }
        private double w = 800; public double W { get { return w; } set { SetProperty(ref w, value); } }
        private double h = 450; public double H { get { return h; } set { SetProperty(ref h, value); } }
        private int xRes = 166; public int XRes { get { return xRes; } set { SetProperty(ref xRes, value); } }
        private int yRes = 94; public int YRes { get { return yRes; } set { SetProperty(ref yRes, value); } }

        private BitmapSource bgImage;

        public BitmapSource BGImage { get { return bgImage; } }

        private BitmapSource fgImage;

        public BitmapSource FGImage { get { return fgImage; } set { SetProperty(ref fgImage, value); } }

        private String _filePath; public String FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        private bool _isPathValid = false;

        private void UpdateFGImage(string imgPath)
        {
            Mat mat = new Mat(imgPath);
            Mat resizeMat = new Mat();
            Cv2.Resize(mat, resizeMat, new OpenCvSharp.Size(xRes, yRes));
            FGImage = BitmapSourceConverter.ToBitmapSource(resizeMat);
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
        }
    }
}

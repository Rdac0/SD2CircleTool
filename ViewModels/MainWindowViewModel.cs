using Microsoft.Windows.Themes;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Numerics;
using System.Security.Cryptography.Pkcs;
using System.Windows;
using System.IO;
using static ImTools.ImMap;
using System.Globalization;
using SD2CircleTool.Views;

namespace SD2CircleTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "SD2 Tools"; public string Title { get { return _title; } set { SetProperty(ref _title, value); } }

        private IRegionManager _regionManager;

        public DelegateCommand ToCircleToolCommand => new(() =>
        {
            _regionManager.RequestNavigate("MainRegion", "CircleTool");
        });

        public DelegateCommand ToClassicinatorCommand => new(() =>
        {
            _regionManager.RequestNavigate("MainRegion", "Classicinator");
        });
        public DelegateCommand ToOnimaiCommand => new(() =>
        {
            MainWindow window;
            window = MainWindow.GetInstance();
            window.Width = window.Width < 1200 ? 1200 : window.Width;
            _regionManager.RequestNavigate("MainRegion", "Onimai2Text");
        });

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _regionManager.RegisterViewWithRegion("MainRegion", typeof(CircleTool));
            _regionManager.RegisterViewWithRegion("MainRegion", typeof(Classicinator));
            _regionManager.RegisterViewWithRegion("MainRegion", typeof(Onimai2Text));
        }
    }
}

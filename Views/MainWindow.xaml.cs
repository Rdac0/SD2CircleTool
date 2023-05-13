using System.Windows;

namespace SD2CircleTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow window;

        public static MainWindow GetInstance()
        {
            return window;
        }

        public MainWindow()
        {
            InitializeComponent();
            window = this;
        }
    }
}

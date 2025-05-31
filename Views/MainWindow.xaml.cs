using Localfy.ViewModels;
using System.Windows;

namespace Localfy
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

    }
}
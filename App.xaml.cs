using Localfy.Services;
using Localfy.ViewModels;
using System.Windows;

namespace Localfy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Create the dialog service instance
            var dialogService = new DialogService();

            // 2. Create the main view model and pass the dialog service to it
            var mainViewModel = new MainViewModel(dialogService);

            // 3. Create the main window and set the DataContext
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            // 4. Show the main window
            mainWindow.Show();
        }
    }

}

using Localfy.Services;
using Localfy.ViewModels;
using Localfy.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Threading;

namespace Localfy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        public App()
        {
            IServiceCollection services = new ServiceCollection();

            // Services
            services.AddSingleton<PlaylistService>();
            services.AddSingleton<AudioPlayerService>();
            services.AddSingleton<DispatcherTimer>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IServiceProvider>(sp => sp);

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<NewPlaylistDialogViewModel>();
            services.AddTransient<EditPlaylistDialogViewModel>();
            services.AddTransient<ErrorDialogViewModel>();
            services.AddTransient<ConfirmDialogViewModel>();

            // Main window
            services.AddSingleton<MainWindow>(s => new MainWindow
            {
                DataContext = s.GetRequiredService<MainViewModel>()
            });

            _serviceProvider = services.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            MainWindow.Show();

        }
    }

}

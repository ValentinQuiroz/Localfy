using MaterialDesignThemes.Wpf;
using Localfy.Models;
using Localfy.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace Localfy.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task ShowMessageAsync(string title, string message)
        {
            var viewModel = new ViewModels.Dialogs.ErrorDialogViewModel(title, message);
            await DialogHost.Show(viewModel, "RootDialog");
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var viewModel = new ViewModels.Dialogs.ConfirmDialogViewModel(title, message);
            var result = await DialogHost.Show(viewModel, "RootDialog");
            return result is true;
        }
        public async Task<bool> ShowNewPlaylistDialogAsync()
        {
            var viewModel = _serviceProvider.GetRequiredService<NewPlaylistDialogViewModel>();
            var result = await DialogHost.Show(viewModel, "RootDialog");
            return result is bool;
        }

        public async Task<bool> ShowEditPlaylistDialogAsync(Playlist playlist)
        {
            var viewModel = ActivatorUtilities.CreateInstance<EditPlaylistDialogViewModel>(_serviceProvider, playlist);
            var result = await DialogHost.Show(viewModel, "RootDialog");
            return result is bool;
        }


        public async Task<T?> ShowDialogAsync<T>(object viewModel)
        {
            var result = await DialogHost.Show(viewModel, "RootDialog");
            return result is T value ? value : default;
        }


       
    }
}

using MaterialDesignThemes.Wpf;
using Localfy.Models;

namespace Localfy.Services
{
    public class DialogService : IDialogService
    {
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
            var viewModel = new ViewModels.Dialogs.NewPlaylistDialogViewModel();
            var result = await DialogHost.Show(viewModel, "RootDialog");
            return result is bool;
        }

        public async Task<bool> ShowEditPlaylistDialogAsync(Playlist playlist)
        {
            var viewModel = new ViewModels.Dialogs.EditPlaylistDialogViewModel(playlist);
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

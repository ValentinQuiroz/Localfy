
using Localfy.Models;

namespace Localfy.Services
{
    public interface IDialogService
    {
        Task ShowMessageAsync(string title, string message);
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task<bool> ShowNewPlaylistDialogAsync();
        Task<bool> ShowEditPlaylistDialogAsync(Playlist playlist);
        Task<T?> ShowDialogAsync<T>(object viewModel);
        
    }
}

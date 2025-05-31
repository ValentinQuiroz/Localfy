using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Localfy.Models;
using Localfy.Services;
using Localfy.Views.Dialogs;
using MaterialDesignThemes.Wpf;

namespace Localfy.ViewModels
{
    public partial class EditPlaylistViewModel : ObservableObject
    {
        private PlaylistService playlistService = new PlaylistService();
        private Playlist editedPlaylist;

        [ObservableProperty]
        private string playlistName;
        [ObservableProperty]
        private string playlistDescription;
        [ObservableProperty]
        private string imagePath;

        public EditPlaylistViewModel(Playlist playlist)
        {
            editedPlaylist = playlist;

            PlaylistName = playlist.Name;
            PlaylistDescription = playlist.Description;
            ImagePath = playlist.ImagePath;
        }

        [RelayCommand]
        public async Task EditPlaylist()
        {
            if (!string.IsNullOrEmpty(PlaylistName))
            {
                editedPlaylist.Name = PlaylistName;
                editedPlaylist.Description = string.IsNullOrEmpty(PlaylistDescription) ? null : PlaylistDescription;
                editedPlaylist.ImagePath = string.IsNullOrEmpty(ImagePath) ? null : ImagePath;

                if (playlistService.SavePlaylist(editedPlaylist))
                {
                    DialogHost.CloseDialogCommand.Execute(editedPlaylist, null);
                }
                else
                {
                    await ShowErrorAsync("Failed to update playlist. Please try again.");
                }
            }
            else
            {
                await ShowErrorAsync("The field 'Name' is required");
            }
        }

        private async Task ShowErrorAsync(string message)
        {
            var dialog = new ErrorDialog
            {
                DataContext = new ErrorDialogViewModel(message)
            };
            await DialogHost.Show(dialog, "ErrorDialog");
        }


        [RelayCommand]
        private void BrowseImage()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (ofd.ShowDialog() == true)
            {
                ImagePath = ofd.FileName;
            }
        }
    }
}

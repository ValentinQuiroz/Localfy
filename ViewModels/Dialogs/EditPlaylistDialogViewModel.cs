using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Localfy.Models;
using Localfy.Services;
using MaterialDesignThemes.Wpf;

namespace Localfy.ViewModels.Dialogs
{
    public partial class EditPlaylistDialogViewModel : ObservableObject
    {

        private readonly PlaylistService playlistService = new();
        private Playlist editedPlaylist;

        [ObservableProperty]
        private string playlistName;
        [ObservableProperty]
        private string playlistDescription;
        [ObservableProperty]
        private string imagePath;
        [ObservableProperty]
        private string errorMessage;

        public EditPlaylistDialogViewModel(Playlist playlist)
        {
            editedPlaylist = playlist;

            PlaylistName = playlist.Name;
            PlaylistDescription = playlist.Description;
            ImagePath = playlist.ImagePath;
        }

        [RelayCommand]
        private void Confirm()
        {
            if (string.IsNullOrEmpty(PlaylistName))
            {
                ErrorMessage = "The field 'Name' is required";
                return;
            }

            //If the field 'Name' is not empty, proceed to modify the playlist
            if (EditPlaylist()) DialogHost.CloseDialogCommand.Execute(true, null);
        }

        private bool EditPlaylist()
        {

            editedPlaylist.Name = PlaylistName;
            editedPlaylist.Description = string.IsNullOrEmpty(PlaylistDescription) ? null : PlaylistDescription;
            editedPlaylist.ImagePath = string.IsNullOrEmpty(ImagePath) ? null : ImagePath;

            if(playlistService.SavePlaylist(editedPlaylist)) return true;
            else
            {
                ErrorMessage = "Failed to update playlist. Please try again.";
                return false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
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

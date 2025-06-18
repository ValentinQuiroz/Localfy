using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Localfy.Models;
using Localfy.Services;
using MaterialDesignThemes.Wpf;

namespace Localfy.ViewModels.Dialogs
{
    public partial class EditPlaylistDialogViewModel : ObservableObject
    {

        private readonly PlaylistService _playlistService;
        private readonly IFileDialogService _fileDialogService;
        private Playlist editedPlaylist;

        [ObservableProperty]
        private string playlistName;
        [ObservableProperty]
        private string playlistDescription;
        [ObservableProperty]
        private string imagePath;
        [ObservableProperty]
        private string errorMessage;

        public EditPlaylistDialogViewModel(Playlist playlist, PlaylistService playlistService, IFileDialogService fileDialogService)
        {
            _playlistService = playlistService;
            _fileDialogService = fileDialogService;

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

            if(_playlistService.SavePlaylist(editedPlaylist)) return true;
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
            string? img = _fileDialogService.OpenImageFile();
            if (!string.IsNullOrEmpty(img))
            {
                ImagePath = img;
            }
        }

    }

}

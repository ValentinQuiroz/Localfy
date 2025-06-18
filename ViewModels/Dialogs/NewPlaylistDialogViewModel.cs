using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Localfy.Models;
using Localfy.Services;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace Localfy.ViewModels.Dialogs
{
    public partial class NewPlaylistDialogViewModel : ObservableObject
    {
        private readonly PlaylistService _playlistService;
        private readonly IFileDialogService _fileDialogService;

        [ObservableProperty]
        private string playlistName;
        [ObservableProperty]
        private string playlistDescription;
        [ObservableProperty]
        private string imagePath;
        [ObservableProperty]
        private string errorMessage;


        public NewPlaylistDialogViewModel(PlaylistService playlistService, IFileDialogService fileDialogService)
        {
            _playlistService = playlistService;
            _fileDialogService = fileDialogService;
        }

        [RelayCommand]
        private void Confirm()
        {
            if (string.IsNullOrEmpty(PlaylistName))
            {
                ErrorMessage = "The field 'Name' is required";
                return;
            }

            //If the field 'Name' is not empty, proceed to create the playlist
            if (CreatePlaylist()) DialogHost.CloseDialogCommand.Execute(true, null);
            
        }
        private bool CreatePlaylist()
        {
            Playlist playlist = new Playlist
                (
                    PlaylistName,
                    new ObservableCollection<Song>(),
                    string.IsNullOrEmpty(PlaylistDescription) ? null : PlaylistDescription,
                    string.IsNullOrEmpty(ImagePath) ? null : ImagePath
                );

            if (_playlistService.SavePlaylist(playlist)) return true;
            else 
            {
                ErrorMessage = "Failed to create playlist. Please try again.";
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

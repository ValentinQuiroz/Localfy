using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Localfy.Models;
using Localfy.Services;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows;

namespace Localfy.ViewModels
{
    public partial class NewPlaylistViewModel : ObservableObject
    {
        private PlaylistService playlistService = new PlaylistService();

        [ObservableProperty]
        private string playlistName;
        [ObservableProperty]
        private string playlistDescription;
        [ObservableProperty]
        private string imagePath;

        public NewPlaylistViewModel()
        {

        }


        [RelayCommand]
        private void CreatePlaylist()
        {
            if(!string.IsNullOrEmpty(PlaylistName))
            {
                Playlist playlist = new Playlist
                (
                    PlaylistName,
                    new ObservableCollection<Song>(),
                    string.IsNullOrEmpty(PlaylistDescription) ? null : PlaylistDescription,
                    string.IsNullOrEmpty(ImagePath) ? null : ImagePath
                );

                if(playlistService.SavePlaylist(playlist))
                {
                    DialogHost.CloseDialogCommand.Execute(playlist, null);
                }
                else
                {
                    MessageBox.Show("Failed to create playlist. Please try again.");
                }
            }
            else MessageBox.Show("The field 'Name' is required");
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

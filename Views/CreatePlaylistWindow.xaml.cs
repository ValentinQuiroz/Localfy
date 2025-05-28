using Localfy.Models;
using Localfy.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Localfy.Views
{


    public partial class CreatePlaylistWindow : Window
    {
        Playlist playlist;
        PlaylistService playlistService;
        public CreatePlaylistWindow()
        {
            InitializeComponent();
        }
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTxt.Text))
            {
                playlist = new Playlist
                (
                    NameTxt.Text,
                    new ObservableCollection<Song>(),
                    DescriptionTxt.Text,
                    string.IsNullOrWhiteSpace(ImagePathTxt.Text) ? null : ImagePathTxt.Text
                );
                playlistService = new PlaylistService();
                playlistService.SavePlaylist(playlist);

                DialogResult = true;
            }
            else
            {
                MessageBox.Show("The field 'Name' is required");
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Imagenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Todos los archivos (*.*)|*.*";
            if (ofd.ShowDialog() == true)
            {
                ImagePathTxt.Text = ofd.FileName;
            }

        }
    }
}

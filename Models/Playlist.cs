using System.Collections.ObjectModel;

namespace Localfy.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
        public string ImagePath { get; set; }

        public Playlist(string name, ObservableCollection<Song> songs, string description = null, string imagePath = null)
        {
            //the id is assigned in the json automatically by the service 
            Id = 0;
            Name = name;
            Songs = songs;
            Description = description;
            ImagePath = imagePath;
        }
    }
}

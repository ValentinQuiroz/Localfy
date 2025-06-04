using Localfy.Models;
using System.IO;
using System.Text.Json;

namespace Localfy.Services
{
    internal class PlaylistService
    {
        //Json file path stores playlist object to keep the references of the songs
        // C:\Users\<USER>\AppData\Roaming\Localfy\Playlists\
        private readonly string jsonFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Localfy", "Playlists");

        private readonly string idFilePath;

        private JsonSerializerOptions serializerOptions = new JsonSerializerOptions { WriteIndented = true };


        public PlaylistService()
        {
            Directory.CreateDirectory(jsonFilePath);
            idFilePath = Path.Combine(jsonFilePath, "lastId.json");
        }


        private int GetNextId()
        {
            int lastId = 0;
            if (File.Exists(idFilePath))
            {
                string content = File.ReadAllText(idFilePath);
                int.TryParse(content, out lastId);
            }

            int newId = lastId + 1;
            File.WriteAllText(idFilePath, newId.ToString());
            return newId;
        }
        public bool SavePlaylist(Playlist playlist)
        {
            if (playlist.Id == 0) playlist.Id = GetNextId();

            string playlistPath = Path.Combine(jsonFilePath, $"playlist-{playlist.Id}.json");

            string jsonString = JsonSerializer.Serialize(playlist, serializerOptions);

            File.WriteAllText(playlistPath, jsonString);

            if (File.Exists(playlistPath)) return true;
            return false;
        }

        public bool DeletePlaylist(Playlist playlist) 
        {
            string playlistPath = Path.Combine(jsonFilePath, $"playlist-{playlist.Id}.json");
            if (File.Exists(playlistPath))
            {
                File.Delete(playlistPath);
                return true;
            }
            return false;
        }

        public Playlist? GetPlaylist(int id)
        {
            string playlistPath = Path.Combine(jsonFilePath, $"playlist-{id}.json");
            if (File.Exists(playlistPath))
            {
                string jsonString = File.ReadAllText(playlistPath);
                return JsonSerializer.Deserialize<Playlist>(jsonString, serializerOptions);
            }
            return null;
        }

        public List<Playlist>? GetAllPlaylist() 
        {
            var playlists = new List<Playlist>();

            foreach (var file in Directory.GetFiles(jsonFilePath, "playlist-*.json"))
            {
                try
                {
                    var playlist = JsonSerializer.Deserialize<Playlist>(File.ReadAllText(file), serializerOptions);
                    if (playlist != null)
                        playlists.Add(playlist);
                }
                catch { } //TEMP SOLUTION ignore invalid files 
            }

            return playlists;
        }

       public TimeSpan GetTotalPlayTime(int id)
       {
            TimeSpan totalPlaytime = TimeSpan.Zero;

            Playlist? playlist = GetPlaylist(id);
            if (playlist != null)
            {
                
                foreach (Song song in playlist.Songs)
                {
                    totalPlaytime += song.Duration;
                }
                
            }
            return totalPlaytime;
       }


    }
}

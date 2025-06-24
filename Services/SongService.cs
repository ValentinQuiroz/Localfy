using Localfy.Models;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Localfy.Services
{
    public class SongService
    {
        // file path stores song album art for every album name
        // C:\Users\<USER>\AppData\Roaming\Localfy\AlbumArt\
        private readonly string albumArtFolderPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Localfy", "AlbumArt");

        // In case the album name is not declared, it will use a default image
        private readonly string defaultAlbumArtPath = Path.Combine(AppContext.BaseDirectory,
            "Assets", "Unknown_Album.jpg");
        public SongService()
        {
            Directory.CreateDirectory(albumArtFolderPath);
        }

        public Song AddSong(string filePath)
        {
            var file = TagLib.File.Create(filePath);
            using var audioFileReader = new MediaFoundationReader(filePath);

            Song song = new Song
            {
                FilePath = filePath,
                Title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),
                Artist = file.Tag.FirstPerformer ?? "Unknown Artist",
                Album = file.Tag.Album ?? "Unknown Album",
                Genre = file.Tag.FirstGenre ?? "Unknown Genre",
                AlbumArt = string.Empty,
                Duration = audioFileReader.TotalTime,
                DurationString = FormatDurationString(audioFileReader.TotalTime)
            };

            SetAlbumArt(song);

            return song;
        }

        private string FormatDurationString(TimeSpan duration)
        {
            return $"{(int)duration.TotalMinutes}:{duration.Seconds:D2}";
        }

        private void SetAlbumArt(Song song)
        {
            string sanitizedAlbumName = SanitizeAlbumName(song.Album);

            if (song.Album == "Unknown Album")
            {
                song.AlbumArt = defaultAlbumArtPath;
                return;
            }

            if (AlbumArtExists(sanitizedAlbumName))
            {
                song.AlbumArt = Path.Combine(albumArtFolderPath, $"{sanitizedAlbumName}.jpg");
            }
            else
            {
                SaveAlbumArt(song);
            }
        }

        private bool AlbumArtExists(string sanitizedAlbumName)
        {
            if (File.Exists(Path.Combine(albumArtFolderPath, $"{sanitizedAlbumName}.jpg"))) return true;
            return false;
        }

        private void SaveAlbumArt(Song song)
        {
            try
            {
                var file = TagLib.File.Create(song.FilePath);

                if (file.Tag.Pictures.Length > 0)
                {
                    var picture = file.Tag.Pictures[0];
                    var albumArtBytes = picture.Data.Data;

                    string sanitizedAlbumName = SanitizeAlbumName(song.Album);
                    string albumArtPath = Path.Combine(albumArtFolderPath, $"{sanitizedAlbumName}.jpg");

                    File.WriteAllBytes(albumArtPath, albumArtBytes);
                    song.AlbumArt = albumArtPath;
                }
                else
                {
                    song.AlbumArt = defaultAlbumArtPath;
                }
            }
            catch (Exception)
            {
                song.AlbumArt = defaultAlbumArtPath;
            }
        }

        private string SanitizeAlbumName(string albumName)
        {
            return string.Join("_", albumName.Split(Path.GetInvalidFileNameChars()));
        }

    }
}


namespace Localfy.Models
{
    public class Song
    {
        public string FilePath { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public string AlbumArt { get; set; }
        public TimeSpan Duration {  get; set; }
        public string DurationString { get; set; }

        public Song(){  }

        public override bool Equals(object? obj)
        {
            if (obj is not Song other) return false;
            return Title == other.Title && FilePath == other.FilePath;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Title, FilePath);
        }
    }
}

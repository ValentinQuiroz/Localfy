using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Localfy.Models
{
    public class Song
    {
        public string Title { get; set; }
        public TimeSpan Duration {  get; set; }
        public string DurationString { get; set; }
        public string FilePath {  get; set; }

        public Song(string title, TimeSpan duration, string filePath)
        {
            Title = title;
            Duration = duration;
            SetDurationString();
            FilePath = filePath;
        }
        
        private void SetDurationString()
        {
            this.DurationString = $"{(int)Duration.TotalMinutes}:{Duration.Seconds:D2}";
        }
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

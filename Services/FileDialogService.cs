using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Localfy.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string[]? OpenAudioFiles()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Audio Files (*.mp3;*.wav)|*.mp3;*.wav",
                Multiselect = true
            };

            return ofd.ShowDialog() == true ? ofd.FileNames : null;
        }

        public string? OpenImageFile()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            return ofd.ShowDialog() == true ? ofd.FileName : null;
        }
    }
}

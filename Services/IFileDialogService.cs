using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Localfy.Services
{
    public interface IFileDialogService
    {
        string[]? OpenAudioFiles();
        string? OpenImageFile();
    }
}

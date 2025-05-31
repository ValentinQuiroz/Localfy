using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Localfy.ViewModels
{
    public partial class ErrorDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        public string message;

        public ErrorDialogViewModel(string _message)
        {
            Message = _message;
        }
    }
}

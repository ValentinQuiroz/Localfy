using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace Localfy.ViewModels.Dialogs
{
    public partial class ErrorDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string dialogTitle;
        [ObservableProperty]
        private string message;

        [RelayCommand]
        private void CloseDialog()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }

        public ErrorDialogViewModel(string title, string message)
        {
            DialogTitle = title;
            Message = message;
        }

    }
}

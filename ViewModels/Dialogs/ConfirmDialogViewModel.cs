using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace Localfy.ViewModels.Dialogs
{
    public partial class ConfirmDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string dialogTitle;
        [ObservableProperty]
        private string message;

        public ConfirmDialogViewModel(string title, string message)
        {
            DialogTitle = title;
            Message = message;
        }

        [RelayCommand]
        private void Confirm()
        {
            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
    }
}

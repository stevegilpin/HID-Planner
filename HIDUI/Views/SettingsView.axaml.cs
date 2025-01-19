using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HIDUI.ViewModels;

namespace HIDUI.Views;

public partial class SettingsView : UserControl
{
    private SettingsViewModel ViewModel => DataContext as SettingsViewModel;
    
    public SettingsView()
    {
        InitializeComponent();
    }
    
    
    public async void ChangeDefaultInputFolderOnClick(object sender, RoutedEventArgs args) {
        var defaultInputsFolder = await this.ChooseFolderPath("Change Default Input Folder Location");
        ViewModel.TrySetDefaultInputFolderLocation(defaultInputsFolder, out string errorMessage);
    }
    
    public async void ChangeDefaultOutputFolderOnClick(object sender, RoutedEventArgs args) {
        var defaultOutputsFolder = await this.ChooseFolderPath("Change Default Output Folder Location");
        ViewModel.TrySetDefaultOutputFolderLocation(defaultOutputsFolder, out string errorMessage);
    }
}
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HIDUI.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace HIDUI.Views;

public partial class SettingsView : UserControl
{
    private SettingsViewModel ViewModel => DataContext as SettingsViewModel;
    
    public SettingsView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ViewModel?.PopulateSettings();
    }
    
    public async void ChangeDefaultInputFolderOnClick(object sender, RoutedEventArgs args) {
        var defaultInputsFolder = await this.ChooseFolderPathAsync("Change Default Input Folder Location");
        ViewModel.TrySetDefaultInputFolderLocation(defaultInputsFolder, out string errorMessage);
    }
    
    public async void ChangeDefaultOutputFolderOnClick(object sender, RoutedEventArgs args) {
        var defaultOutputsFolder = await this.ChooseFolderPathAsync("Change Default Output Folder Location");
        ViewModel.TrySetDefaultOutputFolderLocation(defaultOutputsFolder, out string errorMessage);
    }

    private async void Button_OnClick(object sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Continue?", "You are about to clear the database. A backup will be saved and then this application will close. Do you want to continue?", ButtonEnum.YesNo);
        var res = await box.ShowWindowDialogAsync((Window)VisualRoot);
        if (res != ButtonResult.Yes)
            return;
        
        var backupLocation = ViewModel.ClearDatabase();
        var box2 = MessageBoxManager.GetMessageBoxStandard("Database Cleared", $"The application will exit now.{Environment.NewLine}{Environment.NewLine}The database has been deleted, and a backup was saved here:{Environment.NewLine}{backupLocation}");
        await box2.ShowWindowDialogAsync((Window)VisualRoot);
        Environment.Exit(0);
    }
}
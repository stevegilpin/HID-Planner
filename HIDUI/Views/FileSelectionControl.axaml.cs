using System;
using System.IO;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using HIDUI.ViewModels;

namespace HIDUI.Views;

public partial class FileSelectionControl : UserControl
{
    public static readonly StyledProperty<string> DisplayNameProperty = AvaloniaProperty.Register<FileSelectionControl, string>(nameof(DisplayName));
    public static readonly StyledProperty<ICommand> RemoveFileCommandProperty = AvaloniaProperty.Register<FileSelectionControl, ICommand>(nameof(RemoveFileCommand), enableDataValidation: true);
    public static readonly AttachedProperty<string> FilePathProperty = AvaloniaProperty.RegisterAttached<FileSelectionControl, Interactive, string>(nameof(FilePath), default, false, BindingMode.OneWay);
    
    public FileSelectionControl()
    {
        InitializeComponent();
        FilePathProperty.Changed.AddClassHandler<Interactive>(HandleFilePathChanged);
    }

    private void HandleFilePathChanged(Interactive sender, AvaloniaPropertyChangedEventArgs eventArgs)
    {
        if (sender != this)
            return;
        
        var filePath = sender.GetValue(FilePathProperty);
        if (string.IsNullOrEmpty(filePath) == false)
            OnFileSelected(filePath);
        else
            OnFileDeselected();
    }

    public ICommand RemoveFileCommand {
        get => GetValue(RemoveFileCommandProperty);
        set => SetValue(RemoveFileCommandProperty, value);
    }

    public string DisplayName
    {
        get { return GetValue(DisplayNameProperty); }
        set { SetValue(DisplayNameProperty, value); }
    }

    public string FilePath
    {
        get { return GetValue(FilePathProperty); }
        set { SetValue(FilePathProperty, value); }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        RemoveFileCommand?.Execute(null);
    }

    private void OnFileSelected(string filePath)
    {
        Border.BorderBrush = new SolidColorBrush(Colors.Green);
        FileChoicePanel.IsVisible = true;
        FileNameTextBlock.Text = Path.GetFileName(filePath);
    }
    private void OnFileDeselected()
    {
        Border.BorderBrush = new SolidColorBrush(Colors.Gray);
        FileChoicePanel.IsVisible = false;
        FileNameTextBlock.Text = string.Empty;
    }
}
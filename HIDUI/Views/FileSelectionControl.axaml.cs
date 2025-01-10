using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HIDUI.ViewModels;

namespace HIDUI.Views;

public partial class FileSelectionControl : UserControl
{
    private FileSelectionViewModel ViewModel => DataContext as FileSelectionViewModel;
    public string StatusColor => ViewModel?.IsFileSelected ?? false ? "Green" : "Gray";
    public string DisplayName
    {
        get { return GetValue(DisplayNameProperty); }
        set { SetValue(DisplayNameProperty, value); }
    }
    
    public FileSelectionControl()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<string> DisplayNameProperty = AvaloniaProperty.Register<FileSelectionControl, string>(nameof(DisplayName));
    public static readonly StyledProperty<string> StatusColorProperty = AvaloniaProperty.Register<FileSelectionControl, string>(nameof(StatusColor));
}
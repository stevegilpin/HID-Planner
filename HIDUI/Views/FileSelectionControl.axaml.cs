using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace HIDUI.Views;

public partial class FileSelectionControl : UserControl
{
    public string SelectorName { get; set; }
    
    public FileSelectionControl()
    {
        InitializeComponent();
    }
}
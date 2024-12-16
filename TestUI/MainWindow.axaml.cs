using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TestUI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MyPanel.Children.Add(new MyControl1());
    }

    private void ClickHandler(object sender, RoutedEventArgs e)
    {
        message.Text = "Button clicked!";
        var userControls = MyPanel.Children.OfType<UserControl>().ToList();
        foreach (var c in userControls)
            MyPanel.Children.Remove(c);
        MyPanel.Children.Add(new MyControl2());
    }
}
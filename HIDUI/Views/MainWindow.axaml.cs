using Avalonia.Controls;

namespace HIDUI.Views {

    public partial class MainWindow: Window
    {
        // private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;
        
        public MainWindow() {
            InitializeComponent();
            // needed?
            // DataContext = new MainWindowViewModel(new SchedulingViewModel());
        }
    }
}
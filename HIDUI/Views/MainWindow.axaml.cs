using Avalonia.Controls;
using Avalonia.Interactivity;

namespace HIDUI.Views {

    public partial class MainWindow: Window
    {
        // private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;
        
        public MainWindow() {
            InitializeComponent();
            // needed?
            // DataContext = new MainWindowViewModel(new SchedulingViewModel());
        }

        private void GoToCourseOptionsView_OnClick(object sender, RoutedEventArgs e)
        {
            CourseOptionsView.IsVisible = true;
            SurveyView.IsVisible = false;
            SchedulingView.IsVisible = false;
        }

        private void GoToSurveyView_OnClick(object sender, RoutedEventArgs e)
        {
            SurveyView.IsVisible = true;
            CourseOptionsView.IsVisible = false;
            SchedulingView.IsVisible = false;
        }

        private void GoToSchedulingView_OnClick(object sender, RoutedEventArgs e)
        {
            SchedulingView.IsVisible = true;
            CourseOptionsView.IsVisible = false;
            SurveyView.IsVisible = false;
        }
    }
}
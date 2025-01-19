using Avalonia.Controls;
using Avalonia.Interactivity;

namespace HIDUI.Views {

    public partial class MainWindow: Window
    {
        public MainWindow() {
            InitializeComponent();
        }

        private enum ChildPages
        {
            Unspecified,
            CourseOptions,
            Survey,
            Scheduling,
            Settings,
        }

        private void SelectPage(ChildPages page)
        {
            CourseOptionsView.IsVisible = page == ChildPages.CourseOptions;
            SurveyView.IsVisible = page == ChildPages.Survey;
            SchedulingView.IsVisible = page == ChildPages.Scheduling;
        }

        private void GoToCourseOptionsView_OnClick(object sender, RoutedEventArgs e) => SelectPage(ChildPages.CourseOptions);
        private void GoToSurveyView_OnClick(object sender, RoutedEventArgs e) => SelectPage(ChildPages.Survey);
        private void GoToSchedulingView_OnClick(object sender, RoutedEventArgs e) => SelectPage(ChildPages.Scheduling);
    }
}
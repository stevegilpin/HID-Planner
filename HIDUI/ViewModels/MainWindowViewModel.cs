using System.ComponentModel;
using Avalonia.Threading;

namespace HIDUI.ViewModels;

public partial class MainWindowViewModel: INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private PageViewModelBase currentPage;
        public PageViewModelBase CurrentPage
        {
            get => currentPage;
            set {
                currentPage = value;
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPage)));
                });
            }
        }

        private PageViewModelBase schedulingViewModel = new SchedulingViewModel();
        public PageViewModelBase SchedulingViewModel
        {
            get => schedulingViewModel;
            set {
                schedulingViewModel = value;
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SchedulingViewModel)));
                });
            }
        }

        private PageViewModelBase courseOptionsViewModel = new CourseOptionsViewModel();
        public PageViewModelBase CourseOptionsViewModel
        {
            get => courseOptionsViewModel;
            set {
                courseOptionsViewModel = value;
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CourseOptionsViewModel)));
                });
            }
        }
        private PageViewModelBase surveyViewModel = new SurveyViewModel();
        public PageViewModelBase SurveyViewModel
        {
            get => surveyViewModel;
            set {
                surveyViewModel = value;
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SurveyViewModel)));
                });
            }
        }

        public MainWindowViewModel() {
            // CurrentPage = Pages[1];
            CurrentPage = SchedulingViewModel;
        }

        // public ObservableCollection<PageViewModelBase> Pages { get; } =
        // // private readonly PageViewModelBase[] Pages = 
        // [ 
        //     new SurveyViewModel(),
        //     new SchedulingViewModel()
        // ];

        public void GoToSchedulingView() {
            // Console.WriteLine((Pages[1] as SchedulingViewModel).IsSettingsShown);
            // DataContext = new MainWindowViewModel();
            // CurrentPage = Pages[1];
            CurrentPage = SchedulingViewModel;
        }

        public void GoToSurveyView() {
            // CurrentPage = Pages[0];
            CurrentPage = SurveyViewModel;
        }

        public void GoToCourseOptionsView()
        {
            CurrentPage = CourseOptionsViewModel;
        }
    }
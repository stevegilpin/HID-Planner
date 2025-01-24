using System.ComponentModel;

namespace HIDUI.ViewModels;

public class MainWindowViewModel: INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public SchedulingViewModel SchedulingViewModel { get; } = new();
        public CourseOptionsViewModel CourseOptionsViewModel { get; } = new();
        public SurveyViewModel SurveyViewModel { get; } = new();
        public SettingsViewModel SettingsViewModel { get; } = new();
    }
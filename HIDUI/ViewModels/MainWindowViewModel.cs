using System.ComponentModel;
using Avalonia.Threading;

namespace HIDUI.ViewModels;

public partial class MainWindowViewModel: INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public SchedulingViewModel SchedulingViewModel { get; } = new();
        public CourseOptionsViewModel CourseOptionsViewModel { get; } = new();
        public SurveyViewModel SurveyViewModel { get; } = new();
    }
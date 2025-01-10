using System;
using Avalonia.Collections;

namespace HIDUI.ViewModels
{

    public class SurveyViewModel : ViewModelBase
    {
        public AvaloniaList<int> FailureList { get; set; } = [1, 2, 3];
        public AvaloniaList<int> FailureDetailList { get; set; } = [4, 5, 6];
    }
}

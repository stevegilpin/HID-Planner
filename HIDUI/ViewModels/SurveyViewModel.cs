using System;
using System.IO;
using Avalonia.Collections;
using HIDUI.BusinessLogic;

namespace HIDUI.ViewModels
{

    public class SurveyViewModel : ViewModelBase
    {
        private readonly SurveyReconModel Model = new ();
        public AvaloniaList<int> FailureList { get; set; } = [1, 2, 3];
        public AvaloniaList<int> FailureDetailList { get; set; } = [4, 5, 6];
        
        public bool IsSurveyResponseFilePathSet => string.IsNullOrEmpty(Model.SurveyResponsesFilePath) == false;
        public string SurveyFileName => IsSurveyResponseFilePathSet ? Path.GetFileName(Model.SurveyResponsesFilePath) : string.Empty;
        public string SurveyStatusColor => IsSurveyResponseFilePathSet ? "Green" : "Gray";
        public void RemoveSurveyFile()
        {
            Model.SurveyResponsesFilePath = null;
            BroadcastSurveyFileUpdated();
        }
        private void BroadcastSurveyFileUpdated()
        {
            OnPropertyChanged(nameof(IsSurveyResponseFilePathSet));
            OnPropertyChanged(nameof(SurveyStatusColor));
            OnPropertyChanged(nameof(SurveyFileName));
        }

        public bool IsStudentInfoFilePathSet => string.IsNullOrEmpty(Model.StudentInfoFilePath) == false;
        public string StudentInfoFileName => IsStudentInfoFilePathSet ? Path.GetFileName(Model.StudentInfoFilePath) : string.Empty;
        public string StudentsStatusColor => IsStudentInfoFilePathSet ? "Green" : "Gray";
        public void RemoveStudentsFile()
        {
            Model.StudentInfoFilePath = null;
            BroadcastStudentsFileUpdated();
        }
        private void BroadcastStudentsFileUpdated()
        {
            OnPropertyChanged(nameof(IsStudentInfoFilePathSet));
            OnPropertyChanged(nameof(StudentsStatusColor));
            OnPropertyChanged(nameof(StudentInfoFileName));
        }

        public bool IsCourseInfoFilePathSet => string.IsNullOrEmpty(Model.CourseInfoFilePath) == false;
        public string CourseInfoFileName => IsCourseInfoFilePathSet ? Path.GetFileName(Model.CourseInfoFilePath) : string.Empty;
        public string CoursesStatusColor => IsCourseInfoFilePathSet ? "Green" : "Gray";
        public void RemoveCoursesFile()
        {
            Model.CourseInfoFilePath = null;
            BroadcastCourseFileUpdated();
        }
        private void BroadcastCourseFileUpdated()
        {
            OnPropertyChanged(nameof(IsCourseInfoFilePathSet));
            OnPropertyChanged(nameof(CoursesStatusColor));
            OnPropertyChanged(nameof(CourseInfoFileName));
        }

        public void SetFilePaths(string[] filePaths)
        {
            Model.SetFilePaths(filePaths);
            BroadcastSurveyFileUpdated();
            BroadcastStudentsFileUpdated();
            BroadcastCourseFileUpdated();
        }
    }
}

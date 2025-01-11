using System;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Collections;
using Data.Objects;
using HIDUI.BusinessLogic;

namespace HIDUI.ViewModels
{

    public class SurveyViewModel : ViewModelBase
    {
        private readonly SurveyReconModel Model = new ();
        
        public bool IsSurveyResponseFilePathSet => string.IsNullOrEmpty(Model.SurveyResponsesFilePath) == false;
        public string SurveyFileName => IsSurveyResponseFilePathSet ? Path.GetFileName(Model.SurveyResponsesFilePath) : string.Empty;
        public string SurveyStatusColor => IsSurveyResponseFilePathSet ? "Green" : "Gray";
        public void RemoveSurveyFile()
        {
            Model.SurveyResponsesFilePath = null;
            BroadcastSurveyFileUpdated();
            ClearFailures();
        }
        private void BroadcastSurveyFileUpdated()
        {
            OnPropertyChanged(nameof(IsSurveyResponseFilePathSet));
            OnPropertyChanged(nameof(AllFilePathsSet));
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
            ClearFailures();
        }
        private void BroadcastStudentsFileUpdated()
        {
            OnPropertyChanged(nameof(IsStudentInfoFilePathSet));
            OnPropertyChanged(nameof(AllFilePathsSet));
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
            ClearFailures();
        }
        private void BroadcastCourseFileUpdated()
        {
            OnPropertyChanged(nameof(IsCourseInfoFilePathSet));
            OnPropertyChanged(nameof(AllFilePathsSet));
            OnPropertyChanged(nameof(CoursesStatusColor));
            OnPropertyChanged(nameof(CourseInfoFileName));
        }

        public void SetFilePaths(string[] filePaths)
        {
            Model.SetFilePaths(filePaths);

            BroadcastSurveyFileUpdated();
            BroadcastStudentsFileUpdated();
            BroadcastCourseFileUpdated();

            ClearFailures();
        }

        public bool AllFilePathsSet => IsSurveyResponseFilePathSet && IsStudentInfoFilePathSet && IsCourseInfoFilePathSet;
        public bool FailuresLoaded { get; set; }

        public ObservableCollection<NoResponseFailure> NoResponseFailures { get; } = [];
        public ObservableCollection<SurveyResponse> NoResponseMatchCandidates { get; } = [];
        public ObservableCollection<UnexpectedRespondentFailure> UnexpectedRespondentFailures { get; } = [];
        public ObservableCollection<StudentInfo> UnexpectedRespondentMatchCandidates { get; } = [];
        public ObservableCollection<DuplicateResponseFailure> DuplicateResponseFailures { get; } = [];
        public ObservableCollection<InvalidCourseChoiceFailure> InvalidCourseChoiceFailures { get; } = [];
        public ObservableCollection<IncompleteResponseFailure> IncompleteResponseFailures { get; } = [];

        public void ReconcileFiles()
        {
            ClearFailures();
            Model.LoadAllFailures();

            Model.NoResponseFailures.ForEach(x => NoResponseFailures.Add(x));
            Model.UnexpectedRespondentFailures.ForEach(x => UnexpectedRespondentFailures.Add(x));
            Model.DuplicateResponseFailures.ForEach(x => DuplicateResponseFailures.Add(x));
            Model.InvalidCourseChoiceFailures.ForEach(x => InvalidCourseChoiceFailures.Add(x));
            Model.IncompleteResponseFailures.ForEach(x => IncompleteResponseFailures.Add(x));

            FailuresLoaded = true;
            OnPropertyChanged(nameof(FailuresLoaded));
        }

        private void ClearFailures()
        {
            NoResponseFailures.Clear();
            NoResponseMatchCandidates.Clear();
            UnexpectedRespondentFailures.Clear();
            UnexpectedRespondentMatchCandidates.Clear();
            DuplicateResponseFailures.Clear();
            InvalidCourseChoiceFailures.Clear();
            IncompleteResponseFailures.Clear();

            FailuresLoaded = false;
            OnPropertyChanged(nameof(FailuresLoaded));
        }

        public void UpdateNoResponseCandidates(NoResponseFailure failure)
        {
            NoResponseMatchCandidates.Clear();
            foreach (var candidate in failure.PossibleMatches)
                NoResponseMatchCandidates.Add(candidate);
        }

        public void UpdateUnexpectedResponseCandidates(UnexpectedRespondentFailure failure)
        {
            UnexpectedRespondentMatchCandidates.Clear();
            foreach (var candidate in failure.PossibleMatches)
                UnexpectedRespondentMatchCandidates.Add(candidate);
        }

        public void SaveSchedulingInput(string folder) => Model.SaveAsSchedulingInput(folder);
    }
}

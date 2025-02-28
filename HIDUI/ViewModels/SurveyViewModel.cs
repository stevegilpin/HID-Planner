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
        
        public string SurveyFilePath => Model.SurveyResponsesFilePath;
        public void RemoveSurveyFile()
        {
            Model.SurveyResponsesFilePath = null;
            OnFileInputChanged();
        }

        public string StudentsFilePath => Model.StudentInfoFilePath;
        public void RemoveStudentsFile()
        {
            Model.StudentInfoFilePath = null;
            OnFileInputChanged();
        }

        public string CoursesFilePath => Model.CourseInfoFilePath;
        public void RemoveCoursesFile()
        {
            Model.CourseInfoFilePath = null;
            OnFileInputChanged();
        }

        public void SetFilePaths(string[] filePaths)
        {
            Model.SetFilePaths(filePaths);
            OnFileInputChanged();
        }

        private void OnFileInputChanged()
        {
            OnPropertyChanged(nameof(IsSurveyResponseFilePathSet));
            OnPropertyChanged(nameof(SurveyFilePath));
            OnPropertyChanged(nameof(StudentsFilePath));
            OnPropertyChanged(nameof(CoursesFilePath));
            OnPropertyChanged(nameof(CanSaveSchedulingInputs));
            ClearFailures();
        }

        public bool IsSurveyResponseFilePathSet => string.IsNullOrEmpty(Model.SurveyResponsesFilePath) == false;
        public bool FailuresLoaded { get; set; }
        public bool CanSaveSchedulingInputs => FailuresLoaded &&
                                               !string.IsNullOrEmpty(SurveyFilePath) &&
                                               !string.IsNullOrEmpty(StudentsFilePath) &&
                                               !string.IsNullOrEmpty(CoursesFilePath);
        public string FailureSummary { get; set; } = string.Empty;
        public string FailureSummaryColor { get; set; } = string.Empty;

        public ObservableCollection<NoResponseFailure> NoResponseFailures { get; } = [];
        public ObservableCollection<SurveyResponse> NoResponseMatchCandidates { get; } = [];
        public ObservableCollection<UnexpectedRespondentFailure> UnexpectedRespondentFailures { get; } = [];
        public ObservableCollection<StudentInfo> UnexpectedRespondentMatchCandidates { get; } = [];
        public ObservableCollection<DuplicateResponseFailure> DuplicateResponseFailures { get; } = [];
        public ObservableCollection<InvalidCourseChoiceFailure> InvalidCourseChoiceFailures { get; } = [];
        public ObservableCollection<IncompleteResponseFailure> IncompleteResponseFailures { get; } = [];

        public void ReconcileFiles(bool loadFiles = true)
        {
            ClearFailures();
            Model.LoadAllFailures(loadFiles);

            Model.NoResponseFailures.ForEach(x => NoResponseFailures.Add(x));
            Model.UnexpectedRespondentFailures.ForEach(x => UnexpectedRespondentFailures.Add(x));
            Model.DuplicateResponseFailures.ForEach(x => DuplicateResponseFailures.Add(x));
            Model.InvalidCourseChoiceFailures.ForEach(x => InvalidCourseChoiceFailures.Add(x));
            Model.IncompleteResponseFailures.ForEach(x => IncompleteResponseFailures.Add(x));

            FailuresLoaded = true;
            OnPropertyChanged(nameof(FailuresLoaded));
            OnPropertyChanged(nameof(CanSaveSchedulingInputs));

            int failureCount = Model.NoResponseFailures.Count +
                               Model.UnexpectedRespondentFailures.Count +
                               Model.DuplicateResponseFailures.Count +
                               Model.InvalidCourseChoiceFailures.Count +
                               Model.IncompleteResponseFailures.Count;

            FailureSummary = failureCount == 0 ? "No failures found" : $"{failureCount} failure(s) found";
            FailureSummaryColor = failureCount == 0 ? "Green" : "Red";
            OnPropertyChanged(nameof(FailureSummary));
            OnPropertyChanged(nameof(FailureSummaryColor));
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
            OnPropertyChanged(nameof(CanSaveSchedulingInputs));
            
            FailureSummary = string.Empty;
            FailureSummaryColor = string.Empty;
            OnPropertyChanged(nameof(FailureSummary));
            OnPropertyChanged(nameof(FailureSummaryColor));
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
        
        public void SaveMissingResponsesList(string folder) => Model.SaveMissingResponsesList(folder);

        public void ResolveUnexpectedResponse(UnexpectedRespondentFailure failure, StudentInfo match, bool useSurveyResponse)
        {
            Model.UnexpectedRespondentFailures.Remove(failure);
            if (useSurveyResponse) {
                match.FirstName = failure.Response.FirstName;
                match.LastName = failure.Response.LastName;
            } else {
                failure.Response.FirstName = match.FirstName;
                failure.Response.LastName = match.LastName;
            }

            ReconcileFiles(false);
        }
    }
}

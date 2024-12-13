using System;

namespace HIDUI.ViewModels
{

    public class SurveyViewModel : PageViewModelBase
    {
        public override bool CanNavigateNext 
        { 
            get => false;
            protected set => throw new NotSupportedException(); 
        }

        public override bool CanNavigatePrevious
        {
            get => true;
            protected set => throw new NotSupportedException();
        }
    }
}

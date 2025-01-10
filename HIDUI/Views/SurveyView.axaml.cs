using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HIDUI.ViewModels;

namespace HIDUI.Views
{
    public partial class SurveyView : ContentControl
    {
        private SurveyViewModel ViewModel => DataContext as SurveyViewModel;
        
        public SurveyView()
        {
            InitializeComponent();
        }
        
        private async void ChooseFileOnClick(object sender, RoutedEventArgs e)
        {
            var filePaths = await ChooseFilePath("Select Import Files");
            if (filePaths.Length == 0)
                return;

            ViewModel.SetFilePaths(filePaths);
        }
        
        // TODO create a UI utility. Some of these methods were directly copied from scheduling view.
        public async Task<string[]> ChooseFilePath(string title) {
            var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                Title = title,
                AllowMultiple = true,
            });
            return files.Select(file => file.Path.LocalPath).ToArray();
        }
    }
}

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HIDLib;
using TestApp;

namespace HIDUI.Views {

    public partial class InputField: UserControl {
        private InputFieldViewModel ViewModel => DataContext as InputFieldViewModel;

        public InputField() {
            InitializeComponent();
            DataContext = new InputFieldViewModel();
        }

    }

    public partial class InputFieldViewModel: INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Courses
        private bool isFileSet = false;
        public bool IsFileSet {
            get => isFileSet;
            set {
                isFileSet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFileSet)));
            }
        }
        private string fileName;
        public string FileName {
            get => fileName;
            set {
                fileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
            }
        }

        private string filePath;
        public string FilePath {
            get => filePath;
            set {
                filePath = value;
                FileName = System.IO.Path.GetFileName(FilePath);
                IsFileSet = true;
                StatusColor = "Green";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
            }
        }

        private string statusColor = "Gray";
        public string StatusColor {
            get => statusColor;
            set {
                statusColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusColor)));
            }
        }
    }
}
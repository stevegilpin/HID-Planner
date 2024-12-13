using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Data;
using Data.Objects;
using DynamicData;
using HIDLib;
using HIDLib.Analytics;
using HIDLib.HTML;
using HIDLib.IO;
using HIDLib.ReportGenerators;
using HIDUI.ViewModels;

namespace HIDUI.Views {

    public partial class MainWindow: Window
    {
        // private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;
        
        public MainWindow() {
            InitializeComponent();
            // needed?
            // Console.WriteLine((DataContext as MainWindowViewModel).CurrentPage);
            // DataContext = new MainWindowViewModel(new SchedulingViewModel());
        }
    }

    public partial class MainWindowViewModel: INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        // private PageViewModelBase subViewModel;
        // public PageViewModelBase SubViewModel
        // {
        //     get => subViewModel;
        //     set {
        //         subViewModel = value;
        //         Console.WriteLine("SET SUBVM");
        //         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubViewModel)));
        //     }
        // }

        private PageViewModelBase currentPage;
        public PageViewModelBase CurrentPage
        {
            get => currentPage;
            set {
                currentPage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPage)));
            }
        }

        public MainWindowViewModel() {
            CurrentPage = Pages[1];
            // CurrentPage = page;
        }

        private readonly PageViewModelBase[] Pages = 
        [ 
            new SurveyViewModel(),
            new SchedulingViewModel()
        ];

        public void GoToSchedulingView() {
            // SubViewModel = new SchedulingViewModel();
            Console.WriteLine((Pages[1] as SchedulingViewModel).IsSettingsShown);
            CurrentPage = Pages[1];
        }

        public void GoToSurveyView() {
            // SubViewModel = new SurveyViewModel();
            CurrentPage = Pages[0];
        }
    }
}
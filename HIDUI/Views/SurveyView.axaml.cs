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

namespace HIDUI.Views
{
    public partial class SurveyView : ContentControl
    {
        public SurveyView()
        {
            InitializeComponent();
            // DataContext = new SchedulingViewModel();
            Console.WriteLine("SURVEY VIEW");
        }
    }
}

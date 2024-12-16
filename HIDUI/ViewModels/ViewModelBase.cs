using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HIDUI.ViewModels;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public abstract class PageViewModelBase : ViewModelBase {
    /// <summary>
    /// Gets if the user can navigate to the next page
    /// </summary>
    public abstract bool CanNavigateNext { get; protected set; }

    /// <summary>
    /// Gets if the user can navigate to the previous page
    /// </summary>
    public abstract bool CanNavigatePrevious { get; protected set; }
}

public class MainViewTemplateSelector : IDataTemplate
{
    // This Dictionary should store our shapes. We mark this as [Content], so we can directly add elements to it later.
    [Content]
    public Dictionary<string, IDataTemplate> AvailableViews { get; } = [];

    // The Keys of the DataTemplates for the views.
    private List<string> _ValidViews = ["SurveyView", "SchedulingView"]; // Here are more in reality

    // Build the DataTemplate here
    public Control Build(object? param)
    {
        var key = (param?.ToString()) ?? throw new ArgumentNullException(nameof(param)); // Our Keys in the dictionary are strings, so we call .ToString() to get the key to look up
        return AvailableViews[key].Build(param);// finally we look up the provided key and let the System build the DataTemplate for us
    }

    // Check if we can accept the provided data
    public bool Match(object? data)
    {
        if (data?.GetType() == typeof(string))
        {
            if (_ValidViews.Contains((string)data)) 
                return true;

            return false;
        }
        else
            return false;
    }
}

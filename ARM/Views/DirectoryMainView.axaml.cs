using ARM.Models;
using ARM.Services;
using ARM.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;

namespace ARM;

public partial class DirectoryMainView : Window
{
    public DirectoryMainView(IDBService dbService, string directoryName)
    {
        InitializeComponent();
        DataContext = new DirectoryMainViewModel(directoryName, dbService);

    }
}
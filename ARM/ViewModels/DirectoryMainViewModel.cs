using ARM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ARM.Models;
using ARM.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using Avalonia.Controls;
using Avalonia.Data;

namespace ARM.ViewModels
{
    using ARM.ViewModels.Directories;
    using ARM.Views.Directories;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using System.Collections.ObjectModel;

    internal partial class DirectoryMainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object? currentViewModel;

        public DirectoryMainViewModel(string directoryName, IDBService dbService)
        {
            switch (directoryName)
            {
                case "Posts":
                    CurrentViewModel = new DirectoryPostsViewModel((PostgresDBService)dbService);
                    break;
                case "Products":
                    CurrentViewModel = new DirectoryProductsViewModel((PostgresDBService)dbService);
                    break;
                case "Tanks":
                    CurrentViewModel = new DirectoryTanksViewModel((PostgresDBService)dbService);
                    break;
                case "Tubes":
                    CurrentViewModel = new DirectoryTubesViewModel((PostgresDBService)dbService);
                    break;
                default:
                   // MessageBox.Show("Неизвестный справочник: " + directoryName);
                    break;
            }
        }
    }

}

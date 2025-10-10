using ARM.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ARM.Views.Directories;

public partial class DirectoryTanksView : UserControl
{
    public DirectoryTanksView()
    {
        InitializeComponent();
    }
    public DirectoryTanksView(IDBService dbService)
    : this()
    {
        // тут можно сохранять dbService в поле/свойство, если нужно
    }
}
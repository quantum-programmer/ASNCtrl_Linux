using ARM.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ARM.Views.Directories;

public partial class DirectoryTubesView : UserControl
{
    public DirectoryTubesView()
    {
        InitializeComponent();
    }
    public DirectoryTubesView(IDBService dbService)
    : this()
    {
        // тут можно сохранять dbService в поле/свойство, если нужно
    }

}
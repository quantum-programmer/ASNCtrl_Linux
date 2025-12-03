using ARM.Models;
using ARM.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ARM.ViewModels.Directories
{
    public partial class DirectoryTubesViewModel : ObservableObject
    {
        private readonly PostgresDBService _dbService;

        public ObservableCollection<TubeModel> Tubes { get; } = new();


        private TubeModel selectedTube;
        public TubeModel SelectedTube
        {
            get => selectedTube;
            set => SetProperty(ref selectedTube, value);
        }

        private TubeModel _addedItem;

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }

        public DirectoryTubesViewModel(PostgresDBService dbService)
        {
            _dbService = dbService;

            AddCommand = new RelayCommand(OnAdd);
            SaveCommand = new RelayCommand(async () => await OnSaveAsync());
            DeleteCommand = new RelayCommand(async () => await OnDeleteAsync(), () => SelectedTube != null);
            CancelCommand = new RelayCommand(OnCancel);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var tubes = await _dbService.GetTubesAsync();
            Tubes.Clear();
            foreach (var t in tubes)
                Tubes.Add(t);
        }

        private void OnAdd()
        {
            if (_addedItem != null)
                return;

            _addedItem = new TubeModel();
            Tubes.Add(_addedItem);
            SelectedTube = _addedItem;
        }

        private async Task OnSaveAsync()
        {
            if (SelectedTube == null)
                return;

            bool isNew = ReferenceEquals(SelectedTube, _addedItem);

            if (isNew)
            {
                // Вставка новой строки
                var newId = await _dbService.InsertTubeAsync(SelectedTube);

                // Если база возвращает новый ID — запишем в модель
                if (newId > 0)
                    SelectedTube.Tube = (short)newId;

                _addedItem = null;
            }
            else
            {
                // Обновление существующей строки
                await _dbService.UpdateTubeAsync(SelectedTube);
            }

            await LoadDataAsync(); 
        }


        private async Task OnDeleteAsync()
        {
            if (SelectedTube == null) return;

            await _dbService.DeleteTubeAsync(SelectedTube.Tube);
            Tubes.Remove(SelectedTube);
            SelectedTube = null;
        }

        private void OnCancel()
        {
            if (_addedItem != null)
            {
                Tubes.Remove(_addedItem);
                _addedItem = null;
            }
        }
    }
}

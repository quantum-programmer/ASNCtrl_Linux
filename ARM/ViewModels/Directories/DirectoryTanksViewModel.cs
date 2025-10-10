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
    public partial class DirectoryTanksViewModel : ObservableObject
    {
        private readonly PostgresDBService _dbService;

        public ObservableCollection<TankModel> Tanks { get; } = new();
        public List<ProductModel> Products { get; private set; } = new();

        private TankModel selectedTank;
        public TankModel SelectedTank
        {
            get => selectedTank;
            set => SetProperty(ref selectedTank, value);
        }

        private TankModel _addedItem;

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }

        public DirectoryTanksViewModel(PostgresDBService dbService)
        {
            _dbService = dbService;

            AddCommand = new RelayCommand(OnAdd);
            SaveCommand = new RelayCommand(async () => await OnSaveAsync());
            DeleteCommand = new RelayCommand(async () => await OnDeleteAsync(), () => SelectedTank != null);
            CancelCommand = new RelayCommand(OnCancel);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            Products = await _dbService.GetProductsAsync(); // список ProductModel
            var tanks = await _dbService.GetTanksAsync();

            Tanks.Clear();
            foreach (var t in tanks)
            {
                // назначаем один и тот же список продуктов каждой строке
                t.ProductsLookup = Products;
                // синхронизация LookupProduct произойдёт в сеттере ProductsLookup
                Tanks.Add(t);
            }
        }

        private void OnAdd()
        {
            if (_addedItem != null) return;

            _addedItem = new TankModel
            {
                Tank = 0,
                Point = 0,
                Product = null,
                Name = "",
                MaxH = null,
                LevelSensAddres = null,
                OriginalTank = 0
            };

            _addedItem = new TankModel { ProductsLookup = Products };
            Tanks.Add(_addedItem);
            SelectedTank = _addedItem;
        }

        private async Task OnSaveAsync()
        {
            var item = SelectedTank;
            if (item == null) return;

            var isNew = ReferenceEquals(item, _addedItem) || item.Tank == 0;

            if (isNew)
            {
                var newId = await _dbService.InsertTankAsync(item);
                if (newId > 0)
                {
                    item.Tank = (short)newId;
                    item.OriginalTank = item.Tank;
                }
                _addedItem = null;
            }
            else
            {
                await _dbService.UpdateTankAsync(item);
            }

            await LoadDataAsync();
        }

        private async Task OnDeleteAsync()
        {
            if (SelectedTank == null) return;
            await _dbService.DeleteTankAsync(SelectedTank.Tank);
            Tanks.Remove(SelectedTank);
            SelectedTank = null;
        }

        private void OnCancel()
        {
            if (_addedItem != null)
            {
                Tanks.Remove(_addedItem);
                _addedItem = null;
            }
        }
    }
    }

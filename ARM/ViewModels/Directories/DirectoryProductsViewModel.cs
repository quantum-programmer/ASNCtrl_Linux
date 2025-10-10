using ARM.Models;
using ARM.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace ARM.ViewModels.Directories
{
    public partial class DirectoryProductsViewModel : ObservableObject
    {
        private readonly PostgresDBService _dbService;
        public List<LookupItem> FactWProdTypes { get; } = Lookups.FactWProdTypes;
        public List<LookupItem> ProdColors { get; } = Lookups.ProdColors;

        public DirectoryProductsViewModel(PostgresDBService dbService)
        {
            _dbService = dbService;

            Products = new ObservableCollection<ProductModel>();

            AddCommand = new RelayCommand(OnAdd);
            SaveCommand = new RelayCommand(async () => await OnSaveAsync());
            DeleteCommand = new RelayCommand(async () => await OnDeleteAsync(), () => SelectedProduct != null);
            CancelCommand = new RelayCommand(OnCancel);

            LoadDataAsync();
        }

        public ObservableCollection<ProductModel> Products { get; }

        [ObservableProperty]
        private ProductModel selectedProduct;

        private ProductModel _addedItem; // текущая пустая строка при добавлении

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }

        private async void LoadDataAsync()
        {
            await LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            var products = await _dbService.GetProductsAsync();
            Products.Clear();
            foreach (var p in products)
            {
                // Инициализируем lookup для каждой строки
                p.FactWProdTypesLookup = FactWProdTypes;
                p.ProdColorsLookup = ProdColors;

                p.ProdColorLookUp = ProdColors.FirstOrDefault(x => x.Id == p.ProdColor)?.Name ?? "";
                p.LookupFactWProdType = FactWProdTypes.FirstOrDefault(x => x.Id == p.FactW_ProdType)?.Name ?? "";

                p.OriginalProduct = p.Product;
                Products.Add(p);
            }
        }
        private void OnAdd()
        {
            if (_addedItem != null)
                return;

            _addedItem = new ProductModel
            {
                // важно: дать списки для ComboBox, чтобы они не были пустыми
                FactWProdTypesLookup = FactWProdTypes,
                ProdColorsLookup = ProdColors
            };

            Products.Add(_addedItem);
            SelectedProduct = _addedItem;
        }

        private async Task OnSaveAsync()
        {
            var item = SelectedProduct;
            if (item == null)
                return;

            // новая строка = та, что была создана OnAdd
            var isNew = ReferenceEquals(item, _addedItem) || item.Product == 0;

            if (isNew)
            {
                // если БД генерит Id — верни его из Insert и проставь в модель
                var newProduct = await _dbService.InsertProductAsync(item);
                if (newProduct > 0)
                    item.Product = newProduct;

                _addedItem = null; // помечаем, что это больше не «новая» строка
            }
            else
            {
                await _dbService.UpdateProductAsync(item);
            }
        }
        private async Task OnDeleteAsync()
        {
            if (SelectedProduct == null)
                return;

            await _dbService.DeleteProductAsync(SelectedProduct.Product);
            Products.Remove(SelectedProduct);
            SelectedProduct = null;
        }

        private void OnCancel()
        {
            if (_addedItem != null)
            {
                Products.Remove(_addedItem);
                _addedItem = null;
            }
        }

    }
}

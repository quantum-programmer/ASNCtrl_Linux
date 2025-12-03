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

            _ = LoadProductsAsync();
        }

        public ObservableCollection<ProductModel> Products { get; }

        [ObservableProperty]
        private ProductModel selectedProduct;

        private ProductModel _addedItem; // текущая пустая строка при добавлении

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }

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
            foreach (var item in Products)
            {
                if (item.Product == 0)
                {
                    var newId = await _dbService.InsertProductAsync(item);
                    if (newId > 0)
                        item.Product = item.OriginalProduct = newId;
                }
                else
                {
                    await _dbService.UpdateProductAsync(item);
                    item.OriginalProduct = item.Product;
                }
            }

            _addedItem = null;
            await LoadProductsAsync();
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

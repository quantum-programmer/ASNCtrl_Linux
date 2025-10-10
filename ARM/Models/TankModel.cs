using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM.Models
{
    public partial class TankModel : ObservableObject
    {
        private short tank;
        public short Tank { get => tank; set => SetProperty(ref tank, value); }

        private short point;
        public short Point { get => point; set => SetProperty(ref point, value); }

        // id продукта в таблице DTanks
        private short? product;
        public short? Product
        {
            get => product;
            set
            {
                if (SetProperty(ref product, value))
                {
                    // если ProductsLookup уже есть — обновим SelectedProduct и LookupProduct
                    if (ProductsLookup != null)
                    {
                        SelectedProduct = ProductsLookup.FirstOrDefault(x => x.Product == product);
                        LookupProduct = SelectedProduct?.Name ?? "";
                    }
                }
            }
        }

        private string name = "";
        public string Name { get => name; set => SetProperty(ref name, value); }

        private short? maxH;
        public short? MaxH { get => maxH; set => SetProperty(ref maxH, value); }

        private int? levelSensAddres;
        public int? LevelSensAddres { get => levelSensAddres; set => SetProperty(ref levelSensAddres, value); }

        // список продуктов — назначается из ViewModel при загрузке
        private List<ProductModel> productsLookup = new();
        public List<ProductModel> ProductsLookup
        {
            get => productsLookup;
            set
            {
                if (SetProperty(ref productsLookup, value))
                {
                    // при установке списка — синхронизируем SelectedProduct/LookupProduct
                    SelectedProduct = productsLookup?.FirstOrDefault(x => x.Product == Product);
                    LookupProduct = SelectedProduct?.Name ?? "";
                }
            }
        }

        private string lookupProduct = "";
        public string LookupProduct { get => lookupProduct; set => SetProperty(ref lookupProduct, value); }

        // SelectedProduct — объект, на который будет биндинговаться ComboBox.SelectedItem
        private ProductModel selectedProduct;
        public ProductModel SelectedProduct
        {
            get => selectedProduct;
            set
            {
                if (SetProperty(ref selectedProduct, value))
                {
                    if (value != null)
                    {
                        Product = (short)value?.Product;         // сохраняем id
                        LookupProduct = value.Name;     // отображаемое имя
                    }
                    else
                    {
                        Product = null;
                        LookupProduct = "";
                    }
                }
            }
        }

        // Оригинальный ключ для UPDATE
        public short OriginalTank { get; set; }
    }
}
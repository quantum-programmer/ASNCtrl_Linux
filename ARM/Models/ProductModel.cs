using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM.Models
{
    public class ProductModel
    {
        public int Product { get; set; }                  // Product
        public int OriginalProduct {  get; set; }  
        public string Name { get; set; } = "";       // Name
        public int? FactW_ProdType { get; set; }     // FactW_ProdType
        public string LookupFactWProdType { get; set; } = ""; // Lookup по FactW_ProdType
        public double FactW_K0 { get; set; }
        public double FactW_K1 { get; set; }
        public double FactW_K2 { get; set; }
        public string ShortName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Nomenkl { get; set; } = "";
        public bool JRExclude { get; set; }
        public int? ProdGrp { get; set; }
        public bool IsResultProd { get; set; }
        public int? ProdColor { get; set; }
        public string ProdColorLookUp { get; set; } = ""; // Lookup по ProdColor
        // --- Lookup списки ---
        public List<LookupItem> FactWProdTypesLookup { get; set; }
        public List<LookupItem> ProdColorsLookup { get; set; }

        // --- Выбранные элементы ---
        public LookupItem SelectedFactWProdType
        {
            get => FactWProdTypesLookup?.FirstOrDefault(x => x.Id == FactW_ProdType);
            set
            {
                FactW_ProdType = value?.Id;
                LookupFactWProdType = value?.Name ?? "";
            }
        }

        public LookupItem SelectedProdColor
        {
            get => ProdColorsLookup?.FirstOrDefault(x => x.Id == ProdColor);
            set
            {
                ProdColor = value?.Id;
                ProdColorLookUp = value?.Name ?? "";
            }
        }
    }
}

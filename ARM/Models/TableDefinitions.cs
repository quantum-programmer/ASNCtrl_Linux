using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ARM.Models
{
    public static class TableDefinitions
    {
        public static readonly Dictionary<string, Dictionary<string, Type>> TableSchemas = new()
        {
            ["DPosts"] = new Dictionary<string, Type>
            {
                ["Side"] = typeof(int),
                ["Place"] = typeof(int),
                ["Point"] = typeof(string),
                ["Post"] = typeof(string),
                ["FuelType"] = typeof(string),
                ["Volume"] = typeof(int),
                ["Dose"] = typeof(int),
                ["Side"] = typeof(int),
                ["Earth"] = typeof(int),
                ["MachineType"] = typeof(int)
            },
            ["Products"] = new Dictionary<string, Type>
            {
                ["Product"] = typeof(int),
                ["Name"] = typeof(string),
                ["FactW_ProdType"] = typeof(int),
                ["FactW_K0"] = typeof(decimal),
                ["FactW_K1"] = typeof(decimal),
                ["FactW_K2"] = typeof(decimal),
                ["FullName"] = typeof(string),
                ["ShortName"] = typeof(string),
                ["ProdGrp"] = typeof(int),
                ["IsResultProd"] = typeof(bool),
                ["JRExclude"] = typeof(bool),
                ["ProdColor"] = typeof(int)
            }
            // Можно добавить другие таблицы аналогично
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM.Models
{
    public class LookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public static class Lookups
    {
        //products
        public static List<LookupItem> FactWProdTypes => new()
        {
            new LookupItem { Id = 0, Name = "Прочий, произвольные коэффициенты" },
            new LookupItem { Id = 1, Name = "Топливо" },
            new LookupItem { Id = 2, Name = "Нефть" },
            new LookupItem { Id = 3, Name = "Смазочное масло" }
        };

        public static List<LookupItem> ProdColors => new()
        {
            new LookupItem { Id = 1, Name = "Желтый" },
            new LookupItem { Id = 2, Name = "Коричневый" },
            new LookupItem { Id = 3, Name = "Пурпурный" },
            new LookupItem { Id = 4, Name = "Синий" },
            new LookupItem { Id = 5, Name = "Бирюзовый" },
            new LookupItem { Id = 6, Name = "Красный" },
            new LookupItem { Id = 7, Name = "Зеленый" },
            new LookupItem { Id = 8, Name = "Серый" },
            new LookupItem { Id = 9, Name = "Оранжевый" },
            new LookupItem { Id = 10, Name = "Светло-зеленый" },
            new LookupItem { Id = 11, Name = "Розовый" },
            new LookupItem { Id = 12, Name = "Темно-зеленый" }
        };


        //posts
        public static List<LookupItem> FactWMethods => new()
    {
        new LookupItem { Id = -999, Name = "<нет расчета массы>" }, // NULL заменим фиктивным
        new LookupItem { Id = 0, Name = "Получить значение массы от контроллера" },
        new LookupItem { Id = 1, Name = "Знач.объема * пл-ть, введенную вручную" },
        new LookupItem { Id = 2, Name = "Знач.объема * пл-ть, рассчит. по Р 50.2.075-2010 (ареометр 15°)" },
        new LookupItem { Id = 4, Name = "Знач.объема * пл-ть, рассчит. по Р 50.2.075-2010 (ареометр 20°)" },
        new LookupItem { Id = 5, Name = "Знач.объема * пл-ть от плотномера" },
        new LookupItem { Id = 6, Name = "Знач.объема * пл-ть от плотномера с перерасчетом по Р 50.2.075-2010" },
        new LookupItem { Id = 7, Name = "Знач.объема * пл-ть по таблице от темп-ры и концентрации" },
        new LookupItem { Id = 8, Name = "Знач.объема * пл-ть из резервуара" }
    };

        public static List<LookupItem> Directions => new()
    {
        new LookupItem { Id = 0, Name = "Слив" },
        new LookupItem { Id = 1, Name = "Налив" },
        new LookupItem { Id = 2, Name = "Внутр.перекачка" }
    };

        public static List<LookupItem> MachineTypes => new()
    {
        new LookupItem { Id = 0, Name = "Автоцистерна" },
        new LookupItem { Id = 1, Name = "ЖД-цистерна" },
        new LookupItem { Id = 2, Name = "ТРК" },
        new LookupItem { Id = 3, Name = "Дозатор присадок/пробоотборник" },
        new LookupItem { Id = 4, Name = "Счетчик без дозирования" },
        new LookupItem { Id = 5, Name = "ТРП" },
        new LookupItem { Id = 7, Name = "УСН" }
    };

        public static List<LookupItem> CtrlTypes => new()
    {
        new LookupItem { Id = 0, Name = "ЦБУ" },
        new LookupItem { Id = 1, Name = "БУИ" },
        new LookupItem { Id = 3, Name = "ОЗНА" },
        new LookupItem { Id = 4, Name = "БУС" },
        new LookupItem { Id = 2, Name = "Прочие" }
    };

        public static List<LookupItem> UpDownFills => new()
    {
        new LookupItem { Id = 0, Name = "Верхний" },
        new LookupItem { Id = 1, Name = "Нижний" }
    };

        public static List<LookupItem> UserTypedTemperatures => new()
    {
        new LookupItem { Id = 0, Name = "Запрашивается от контроллера" },
        new LookupItem { Id = -1, Name = "Вводится вручную" }
    };

        public static List<LookupItem> StartReverseds => new()
    {
        new LookupItem { Id = 0, Name = "Разрешение - с компьютера, пуск - с кнопки" },
        new LookupItem { Id = 1, Name = "Разрешение - с кнопки, пуск - с компьютера" },
        new LookupItem { Id = 2, Name = "Разрешение и пуск - с компьютера" }
    };

        public static List<LookupItem> FactVMethods => new()
    {
        new LookupItem { Id = 0, Name = "От контроллера" },
        new LookupItem { Id = 1, Name = "По метрштоку и табл.калибр. ж.д.цистерн" }
    };
    }
}

using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM.Models
{
    public class PostModel
    {
        //public int Post { get; set; }
        public int id { get; set; }
        public string VehicleNumber { get; set; }
        public string DriverName { get; set; }
        public string FuelType { get; set; }
        public int Volume { get; set; }
        public int Dose { get; set; }
        //public int Side { get; set; }
        public int Earthed { get; set; }
       // public int MachineType { get; set; }

        public string VolumeInfo => $"{Volume}л / {Dose}л";

        public IRelayCommand? SelectPostCommand { get; set; }


        public short Place { get; set; }
        public short Post { get; set; }
        public short? Point { get; set; }
        public short? Side { get; set; }


        public short? FactVMethod { get; set; }
        public string LookupFactVMethod { get; set; } = "";

        public short? FactWMethod { get; set; }
        public string LookupFactWMethod { get; set; } = "";

        public short? Direction { get; set; }
        public string LookupDirection { get; set; } = "";

        public short? MachineType { get; set; }
        public string LookupMachineType { get; set; } = "";

        public bool? UserTypedTemperature { get; set; }
        public string LookupUserTypedTemperature { get; set; } = "";

        public short? UpDownFill { get; set; }
        public string LookupUpDownFill { get; set; } = "";

        public short? StartReversed { get; set; }
        public string LookupStartReversed { get; set; } = "";

        public short? CtrlType { get; set; }
        public string LookupCtrlType { get; set; } = "";

        public bool? KMXFill { get; set; }
        public short? IsEPost { get; set; }

        public short? Density { get; set; }
        public short? Temperature { get; set; }
        public short? LabDensity { get; set; }
        public short? LabTemperature { get; set; }
        public int? Tank { get; set; }
        public int? HydroMeter { get; set; }
        public int? MType { get; set; }
        public long? LastRecordBUIJournal { get; set; }
        public decimal? TotalVLast { get; set; }

        // Для отслеживания, новая строка или нет
        public int OriginalPost { get; set; }


        // --- Lookup источники ---
        public List<LookupItem> FactVMethodsLookup { get; set; }
        public List<LookupItem> FactWMethodsLookup { get; set; }
        public List<LookupItem> DirectionsLookup { get; set; }
        public List<LookupItem> MachineTypesLookup { get; set; }
        public List<LookupItem> CtrlTypesLookup { get; set; }
        public List<LookupItem> UpDownFillsLookup { get; set; }
        public List<LookupItem> UserTypedTemperaturesLookup { get; set; }
        public List<LookupItem> StartReversedsLookup { get; set; }

        // --- SelectedItem для ComboBox ---
        //public LookupItem SelectedFactVMethod
        //{
        //    get => FactVMethodsLookup.FirstOrDefault(x => x.Id == (FactVMethod ?? 0));
        //    set
        //    {
        //        if (value != null)
        //            FactVMethod = (short)value.Id;
        //        LookupFactVMethod = value?.Name ?? "";
        //    }
        //}

        public LookupItem SelectedFactVMethod
        {
            get => FactVMethodsLookup?.FirstOrDefault(x => x.Id == FactVMethod);
            set
            {
                    FactVMethod = (short)value?.Id;
                LookupFactVMethod = value?.Name ?? "";
            }
        }

        //public LookupItem SelectedFactWMethod
        //{
        //    get => FactWMethodsLookup.FirstOrDefault(x => x.Id == (FactWMethod ?? 0));
        //    set
        //    {
        //        if (value != null)
        //            FactWMethod = (short)value.Id;
        //        LookupFactWMethod = value?.Name ?? "";
        //    }
        //}

        public LookupItem SelectedFactWMethod
        {
            get => FactWMethodsLookup?.FirstOrDefault(x => x.Id == FactWMethod);
            set
            { 
                    FactWMethod = (short)value?.Id;
                LookupFactWMethod = value?.Name ?? "";
            }
        }



        //public LookupItem SelectedDirection
        //{
        //    get => DirectionsLookup.FirstOrDefault(x => x.Id == (Direction ?? 0));
        //    set
        //    {
        //        if (value != null)
        //            Direction = (short)value.Id;
        //        LookupDirection = value?.Name ?? "";
        //    }
        //}

        public LookupItem SelectedDirection
        {
            get => DirectionsLookup?.FirstOrDefault(x => x.Id == Direction);
            set
            { 
                    Direction = (short)value?.Id;
                LookupDirection = value?.Name ?? "";
            }
        }

        //public LookupItem SelectedMachineType
        //{
        //    get => MachineTypesLookup.FirstOrDefault(x => x.Id == (MachineType ?? 0));
        //    set
        //    {
        //        if (value != null)
        //            MachineType = (short)value.Id;
        //        LookupMachineType = value?.Name ?? "";
        //    }
        //}

        public LookupItem SelectedMachineType
        {
            get => MachineTypesLookup?.FirstOrDefault(x => x.Id == MachineType);
            set
            { 
                    MachineType = (short)value?.Id;
                LookupMachineType = value?.Name ?? "";
            }
        }

        //public LookupItem SelectedCtrlType
        //{
        //    get => CtrlTypesLookup.FirstOrDefault(x => x.Id == (CtrlType ?? 0));
        //    set
        //    {
        //        if (value != null)
        //            CtrlType = (short)value.Id;
        //        LookupCtrlType = value?.Name ?? "";
        //    }
        //}


        public LookupItem SelectedCtrlType
        {
            get => CtrlTypesLookup?.FirstOrDefault(x => x.Id == CtrlType);
            set
            {
                    CtrlType = (short)value?.Id;
                LookupCtrlType = value?.Name ?? "";
            }
        }

        //public LookupItem SelectedUpDownFill
        //{
        //    get => UpDownFillsLookup.FirstOrDefault(x => x.Id == (UpDownFill ?? 0));
        //    set
        //    {
        //        if (value != null)
        //            UpDownFill = (short)value.Id;
        //        LookupUpDownFill = value?.Name ?? "";
        //    }
        //}


        public LookupItem SelectedUpDownFill
        {
            get => UpDownFillsLookup?.FirstOrDefault(x => x.Id == UpDownFill);
            set
            {
                    UpDownFill = (short)value?.Id;
                LookupUpDownFill = value?.Name ?? "";
            }
        }

        //public LookupItem SelectedUserTypedTemperature
        //{
        //    get => UserTypedTemperaturesLookup.FirstOrDefault(x => x.Id == (UserTypedTemperature == true ? 1 : 0));
        //    set
        //    {
        //        if (value != null)
        //            UserTypedTemperature = value.Id == 1;
        //        LookupUserTypedTemperature = value?.Name ?? "";
        //    }
        //}

        public LookupItem SelectedUserTypedTemperature
        {
            get => UserTypedTemperaturesLookup?
                       .FirstOrDefault(x =>
                           (UserTypedTemperature == true && x.Id == -1) ||
                           (UserTypedTemperature == false && x.Id == 0));

            set
            {
                if (value == null)
                {
                    UserTypedTemperature = null;
                    LookupUserTypedTemperature = "";
                }
                else
                {
                    // маппим обратно
                    UserTypedTemperature = value.Id == -1;
                    LookupUserTypedTemperature = value.Name;
                }
            }
        }

        //public LookupItem SelectedStartReversed
        //{
        //    get => StartReversedsLookup.FirstOrDefault(x => x.Id == (StartReversed ?? 0));
        //    set
        //    {
        //        if (value != null)
        //            StartReversed = (short)value.Id;
        //        LookupStartReversed = value?.Name ?? "";
        //    }
        //}

        public LookupItem SelectedStartReversed
        {
            get => StartReversedsLookup?.FirstOrDefault(x => x.Id == StartReversed);
            set
            { 
                    StartReversed = (short)value?.Id;
                LookupStartReversed = value?.Name ?? "";
            }
        }

    }
}

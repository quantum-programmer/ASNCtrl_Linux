using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ARM.Models
{
    public class ARMReport
    {
        public int ARMReportID { get; set; }
        public string? Name { get; set; }
        public IAsyncRelayCommand? OpenCommand { get; set; }
    }
}

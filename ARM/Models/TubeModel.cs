using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM.Models
{
    public partial class TubeModel : ObservableObject
    {
        public short Tube { get; set; }
        public short Point1 { get; set; }
        public short Point2 { get; set; }
    }
}

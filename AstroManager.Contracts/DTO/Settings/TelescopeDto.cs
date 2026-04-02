using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model.DTO.Settings
{
    public class TelescopeDto
    {
        public Guid Id { get; set; }
        public string NameTelescope { get; set; }
        public double Aperture { get; set; } // in mm
        public double FocalLength { get; set; } // in mm
        public double BarlowReducer { get; set; } = 1.0; // Multiplier: >1 for barlow, <1 for reducer
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model.DTO.Settings
{
    public class CameraDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double PixelSize { get; set; } // in um
        public int PixelX { get; set; }
        public int PixelY { get; set; }
        public bool IsMono { get; set; }
        public double SkyPA { get; set; } = 0; // Sky Position Angle in degrees
        public bool ManualFilterChanges { get; set; }
        public EManualFilterChangeSource ManualFilterChangeSource { get; set; } = EManualFilterChangeSource.ManualFilterWheelInNina;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Model.Common
{
    public enum CoordinateFormat
    {
        // Hours (or degrees) with decimal places (e.g., 12.3456h or 45.6789°)
        Decimal,

        // Sexagesimal format with colons (e.g., 12:20:44.6 or 45:40:44)
        Sexagesimal,

        // Space-separated format (e.g., 12 20 44.6 or 45 40 44)
        SpaceSeparated,

        // Degrees, minutes, seconds format with symbols (e.g., 12h 20m 44.6s or 45° 40' 44")
        DMS,

        // Compact format without spaces (e.g., 122044.6 or 454044)
        Compact
    }
}

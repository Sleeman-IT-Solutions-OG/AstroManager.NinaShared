using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Shared.Model.Converters;
using Shared.Model.Enums;

namespace Shared.Model.DTO.Settings
{
    public class FovCalculationResult
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public string DisplayString { get; set; } = string.Empty;
    }


    public class EquipmentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string NameOfEquipment { get; set; }
        public CameraDto Camera { get; set; } = new CameraDto();
        public TelescopeDto Telescope { get; set; } = new TelescopeDto();
        public bool HasRotator { get; set; }
        public List<ECameraFilter> Filters { get; set; } = new();
        
        /// <summary>
        /// Default exposure times (in seconds) per filter for this equipment
        /// Key: ECameraFilter, Value: Exposure time in seconds
        /// </summary>
        [JsonConverter(typeof(DictionaryEnumKeyConverter<ECameraFilter, int>))]
        public Dictionary<ECameraFilter, int> DefaultExposureTimes { get; set; } = new();
        
        // FOV Overlay Settings
        public bool ShowAsOverlay { get; set; } = true;
        public bool IsOverlayVisible { get; set; } = false;
        public string OverlayColor { get; set; } = "#ff0000";
        public double OverlayRotationDegrees { get; set; } = 0;
        public bool IsPositionLocked { get; set; } = false;
        public double LockedRA { get; set; } = 0;
        public double LockedDec { get; set; } = 0;
        
        // Mosaic Settings
        public bool EnableMosaic { get; set; } = false;
        public int MosaicPanelsX { get; set; } = 2;
        public int MosaicPanelsY { get; set; } = 2;
        public double MosaicOverlapPercent { get; set; } = 10.0;
        public bool MosaicUseRotator { get; set; } = false;
        
        // Filter Name Mapping (NINA filter name -> AM filter name)
        /// <summary>
        /// Maps NINA-reported filter names to AstroManager filter names.
        /// Key: NINA filter name (as reported by filter wheel), Value: AM filter name
        /// Example: { "L": "Luminance", "Ha": "H-alpha" }
        /// </summary>
        public Dictionary<string, string> FilterNameMappings { get; set; } = new();
        
        /// <summary>
        /// Filter names as reported by NINA during the last connection.
        /// Populated automatically from heartbeat data when a client with this equipment connects.
        /// Used to populate the mapping UI dropdown.
        /// </summary>
        public List<string> NinaReportedFilterNames { get; set; } = new();

        // Read-only properties for calculated values (excluded from serialization)
        [JsonIgnore]
        public double FocalRatio
        {
            get
            {
                if (Telescope == null || Telescope.Aperture <= 0)
                    return 0;
                    
                return Telescope.FocalLength / Telescope.Aperture;
            }
        }

        [JsonIgnore]
        public double PixelScale
        {
            get
            {
                // Null checks
                if (Telescope == null || Camera == null)
                    return 0;
                    
                // Pixel scale (arcseconds/pixel) = (pixel size (μm) * 206.265) / effective focal length (mm)
                var effectiveFocalLength = Telescope.FocalLength * Telescope.BarlowReducer;
                
                // Handle division by zero or invalid values
                if (effectiveFocalLength <= 0 || Camera.PixelSize <= 0)
                    return 0;
                
                var result = (Camera.PixelSize * 206.265) / effectiveFocalLength;
                
                // Check for invalid results
                if (double.IsNaN(result) || double.IsInfinity(result))
                    return 0;
                    
                return result;
            }
        }

        [JsonIgnore]
        public string FOV
        {
            get
            {
                // Null check
                if (Camera == null)
                    return "0.00° × 0.00°";
                    
                // FOV = (sensor size in pixels * pixel scale) in both dimensions
                double fovX = Camera.PixelX * PixelScale;
                double fovY = Camera.PixelY * PixelScale;
                
                // Convert to degrees (divide by 3600 since PixelScale is in arcseconds)
                fovX /= 3600;
                fovY /= 3600;
                
                // Check for invalid values
                if (double.IsNaN(fovX) || double.IsInfinity(fovX)) fovX = 0;
                if (double.IsNaN(fovY) || double.IsInfinity(fovY)) fovY = 0;
                
                return $"{fovX:F2}° × {fovY:F2}°";
            }
        }

        [JsonIgnore]
        public string PixelScaleString
        {
            get
            {
                return $"{PixelScale:F2}\"/px";
            }
        }

        [JsonIgnore]
        public string EffectivePAString 
        {
            get
            {
                // Null check
                if (Camera == null)
                    return "0.0°";
                    
                // calculate effective PA
                var effectivePA = Camera.SkyPA + OverlayRotationDegrees;
                
                // check for invalid values
                if (double.IsNaN(effectivePA) || double.IsInfinity(effectivePA)) effectivePA = 0;
                
                return $"{effectivePA:F1}°";
            }
        }

        [JsonIgnore]
        public string DisplayStringCompact => Telescope != null && Camera != null 
            ? $"{NameOfEquipment} ({Telescope.NameTelescope} | {Camera.Name})"
            : NameOfEquipment ?? "Unknown Equipment";
        
        [JsonIgnore]
        public string DisplayStringSizeDataCompact => GetSizeDataTextCompact();

        public FovCalculationResult CalculateEquipmentFOV()
        {
            // Null check
            if (Camera == null)
            {
                return new FovCalculationResult
                {
                    Width = 0,
                    Height = 0,
                    DisplayString = "0.00° × 0.00°"
                };
            }
            
            // FOV = (sensor size in pixels * pixel scale) in both dimensions
            double fovX = Camera.PixelX * PixelScale;
            double fovY = Camera.PixelY * PixelScale;

            // Convert to degrees (divide by 3600 since PixelScale is in arcseconds)
            fovX /= 3600;
            fovY /= 3600;

            // Check for invalid values
            if (double.IsNaN(fovX) || double.IsInfinity(fovX)) fovX = 0;
            if (double.IsNaN(fovY) || double.IsInfinity(fovY)) fovY = 0;

            // Convert to arcminutes
            var widthArcmin = fovX * 60;
            var heightArcmin = fovY * 60;
            
            // Ensure final values are valid
            if (double.IsNaN(widthArcmin) || double.IsInfinity(widthArcmin)) widthArcmin = 0;
            if (double.IsNaN(heightArcmin) || double.IsInfinity(heightArcmin)) heightArcmin = 0;

            return new FovCalculationResult
            {
                Width = widthArcmin,
                Height = heightArcmin,
                DisplayString = FOV
            };
        }



        public string GetFOVLabelText()
        {
            // Calculate equivalent PA angle (360 - PA)
            var effectivePA = Camera?.SkyPA + OverlayRotationDegrees ?? 0;
            if (double.IsNaN(effectivePA) || double.IsInfinity(effectivePA)) effectivePA = 0;
            
            // Normalize to 0-360 range
            effectivePA = ((effectivePA % 360) + 360) % 360;
            
            // Calculate equivalent angle (mirror)
            var equivalentPA = effectivePA == 0 ? 0 : 360 - effectivePA;
            
            return $"{NameOfEquipment} • FOV: {FOV} • {PixelScaleString} • PA: {effectivePA:F1}° ({equivalentPA:F1}°)";
        }

        public string GetSizeDataTextCompact()
        {
            return $"{FOV} • {PixelScaleString}";
        }

    }
}

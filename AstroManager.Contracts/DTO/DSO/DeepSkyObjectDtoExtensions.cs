using Shared.Model.DTO.Master;
using System;
using System.Collections.Generic;

namespace Shared.Model.DTO.DSO
{
    public static class DeepSkyObjectDtoExtensions
    {
        /// <summary>
        /// Creates a dummy DeepSkyObjectDto from coordinates for display purposes
        /// </summary>
        /// <param name="rightAscension">Right Ascension in degrees</param>
        /// <param name="declination">Declination in degrees</param>
        /// <param name="constellation">Optional constellation name</param>
        /// <param name="customName">Optional custom name, defaults to coordinate display</param>
        /// <param name="id">Optional ID to use, defaults to Guid.Empty</param>
        /// <returns>A DeepSkyObjectDto representing the coordinate position</returns>
        public static DeepSkyObjectDto CreateFromCoordinates(
            double rightAscension, 
            double declination, 
            string? constellation = null, 
            string? customName = null,
            Guid? id = null)
        {
            var displayName = customName ?? $"Position: RA {rightAscension / 15:F2}h, Dec {declination:F2}°";
            
            return new DeepSkyObjectDto
            {
                Id = id ?? Guid.Empty, // Use provided ID or empty GUID to indicate this is a coordinate-only object
                RightAscension = rightAscension,
                Declination = declination,
                Names = new List<ObjectNameDto>
                {
                    new ObjectNameDto
                    {
                        Name = displayName,
                        Priority = 1,
                        Catalog = "Coordinates"
                    }
                },
                SimbadObjectType = new SimbadObjectTypeDto
                {
                    BaseType = "Position",
                    Description = "Coordinate Position"
                },
                Constellation = constellation ?? "N/A"
            };
        }

        /// <summary>
        /// Creates a DeepSkyObjectDetailDto wrapper for coordinate-based objects
        /// </summary>
        /// <param name="rightAscension">Right Ascension in degrees</param>
        /// <param name="declination">Declination in degrees</param>
        /// <param name="constellation">Optional constellation name</param>
        /// <param name="customName">Optional custom name</param>
        /// <param name="id">Optional ID to use, defaults to Guid.Empty</param>
        /// <returns>A DeepSkyObjectDetailDto with the coordinate-based DSO</returns>
        public static DeepSkyObjectDetailDto CreateDetailFromCoordinates(
            double rightAscension, 
            double declination, 
            string? constellation = null, 
            string? customName = null,
            Guid? id = null)
        {
            var dso = CreateFromCoordinates(rightAscension, declination, constellation, customName, id);
            
            return new DeepSkyObjectDetailDto
            {
                DSO = dso,
                ChartData = null // Will be populated if needed
            };
        }
    }
}

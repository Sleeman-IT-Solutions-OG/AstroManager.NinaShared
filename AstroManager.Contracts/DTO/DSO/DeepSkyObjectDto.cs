using Shared.Model.Common;
using Shared.Model.DTO.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace Shared.Model.DTO.DSO
{
    public class DeepSkyObjectDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Identifier { get; set; } = string.Empty;

        public string? ForeignOID { get; set; }

        public string? PrimaryName { get; set; }

        public string? DisplayName { get; set; }

        public string? NameList { get; set; }

        public SimbadObjectTypeDto? SimbadObjectType { get; set; }

        public double RightAscension { get; set; } // Hours
        public double Declination { get; set; } // Degrees

        [MaxLength(10)]
        public string? Constellation { get; set; } // IAU Abbreviation (e.g., And, Ori)

        public double? MagnitudeV { get; set; } // visual magnitude
        public double? MagnitudeP { get; set; } // photographic magnitude

        public double? Distance { get; set; } // distance in ly

        public double? SizeMaxArcmin { get; set; } // Major axis in arcminutes
        public double? SizeMinArcmin { get; set; } // Minor axis in arcminutes
        public double? PositionAngle { get; set; } // Position angle in degrees
        public string? MorphType { get; set; }

        public int? Popularity { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for object names
        public ICollection<ObjectNameDto> Names { get; set; } = new List<ObjectNameDto>();


        // Helper method to get the primary name (highest priority)
        public string? GetPrimaryName()
        {
            if (Names == null || !Names.Any())
                return null;

            string primaryId = Names.OrderBy(n => n.Priority).FirstOrDefault(n => n.Priority > 1)?.Name ?? "";
            string? displayName = GetDisplayName();

            string primaryName = primaryId;

            if (!string.IsNullOrEmpty(displayName) && string.IsNullOrEmpty(primaryName))
                primaryName = $"{displayName}";

            else if (!string.IsNullOrEmpty(displayName))
                primaryName = primaryName + $" ({displayName})";

            return primaryName;
        }

        /// <summary>
        /// Gets the best display name for the object, preferring longer, more descriptive names
        /// over catalog designations (e.g., "Bode's Galaxy" is preferred over "M81")
        /// </summary>
        public string? GetDisplayName()
        {
            if (Names == null || !Names.Any())
                return null;

            // Get all names with priority 1 (highest priority)
            var candidateNames = Names
                .Where(n => n.Priority == 1)
                .Select(n => n.Name?.Trim())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            if (!candidateNames.Any())
                return null;

            // Score each name based on desirability
            var scoredNames = candidateNames
                .Select(name => new
                {
                    Name = name,
                    Score = CalculateNameScore(name)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Name?.Length ?? 0) // Longer names first if scores are equal
                .ToList();

            return scoredNames.FirstOrDefault()?.Name;
        }

        public string? GetDisplayNameWithMatchingName(string? matchingName)
        {
            string? displayName = GetPrimaryName();

            if (string.IsNullOrEmpty(displayName))
                displayName = this.Identifier;

            if (string.IsNullOrEmpty(matchingName))
                return displayName.Replace("NAME ", "");

            else
                return matchingName.Replace("NAME ", "");
        }

        /// <summary>
        /// Calculates a score for a name based on how good it is as a display name
        /// Higher scores are better. Criteria:
        /// - Names with spaces (multi-word) get higher scores
        /// - Names that are just catalog identifiers (M, NGC, IC, etc.) get lower scores
        /// - Names with apostrophes (like "Bode's") get higher scores
        /// </summary>
        private int CalculateNameScore(string name)
        {
            if (string.IsNullOrEmpty(name))
                return 0;

            int score = 0;

            // Prefer names with spaces (multi-word names)
            if (name.Contains(' '))
                score += 2;


            // Penalize catalog identifiers
            if (IsCatalogDesignation(name))
                score -= 5;

            // Slight preference for longer names
            score += Math.Min(name.Length / 10, 2); // Cap at +2 for very long names

            return score;
        }

        /// <summary>
        /// Checks if a name is just a catalog designation (like M81, NGC 1234, etc.)
        /// </summary>
        private bool IsCatalogDesignation(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            // Common catalog prefixes
            string[] catalogPrefixes = { "M ", "M-", "NGC ", "IC ", "UGC ", "PGC ", "PK ", "Abell ", "Sh2-", "LDN ", "LBN " };

            // Check if name matches any catalog pattern
            return catalogPrefixes.Any(prefix =>
                name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                Regex.IsMatch(name, @"^[A-Za-z]+\d+$") // Matches patterns like M81, NGC1234
            );
        }

        public string GetNameList()
        {
            if (Names == null || !Names.Any())
                return "";

            // Get all names except the primary display name (priority 1)
            var names = Names
                //   .Where(n => n.Priority > 1) // Exclude primary display name
                .OrderBy(n => n.Priority)    // Sort by priority
                .ThenBy(n => n.Name)         // Then by name for same priority
                .Select(n => n.Name)
                .Distinct()                  // Remove duplicates
                .ToList();

            return names.Any() ? string.Join(", ", names) : "";
        }

        /// <summary>
        /// Gets a formatted string representation of the object's size in the most appropriate unit.
        /// </summary>
        /// <returns>Formatted size string (e.g., "2.3° x 1.1°", "45' x 30'", "1200\" x 900\"", or "Unknown")</returns>
        public string GetFormattedSize()
        {
            if (SizeMaxArcmin == null || SizeMaxArcmin <= 0)
                return "Unknown";

            // If SizeMinArcmin is not set, use SizeMaxArcmin for both dimensions
            double maxSize = SizeMaxArcmin.Value;
            double minSize = SizeMinArcmin ?? maxSize;

            // If either dimension is zero, use the max size for both (circular object)
            if (minSize <= 0)
                minSize = maxSize;

            // Check if we should show degrees (if max size is >= 60 arcminutes)
            if (maxSize >= 60)
            {
                return $"{maxSize / 60:0.0}°" +
                       (Math.Abs(maxSize - minSize) > 0.1 ? $" x {minSize / 60:0.0}°" : "");
            }
            // Check if we should show arcseconds (if max size is < 1 arcminute)
            else if (maxSize < 1)
            {
                return $"{maxSize * 60:0}\"" +
                       (Math.Abs(maxSize - minSize) > 0.0167 ? $" x {minSize * 60:0}\"" : "");
            }
            // Default to arcminutes
            else
            {
                return $"{maxSize:0.0}'" +
                       (Math.Abs(maxSize - minSize) > 0.1 ? $" x {minSize:0.0}'" : "");
            }
        }

        public string GetFormattedSizeSingle()
        {
            var major = SizeMaxArcmin;
            var minor = SizeMinArcmin;

            if (major.HasValue)
            {
                return $"{major.Value:F1}'";
            }
            else if (minor.HasValue)
            {
                return $"{minor.Value:F1}'";
            }

            return "N/A";
        }


        // Helper method to add a name with a specific priority
        public void AddName(string name, int priority)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                string catalog = "Unknown";
                int prioAuto = 0;

                // Determine catalog and priority based on name prefix/pattern
                if (name.StartsWith("NAME "))
                {
                    catalog = "COMMON";
                    prioAuto = 1;
                }
                else if (name.StartsWith("M "))
                {
                    catalog = "MESSIER";
                    prioAuto = 2;
                }
                else if (name.StartsWith("NGC "))
                {
                    catalog = "NGC";
                    prioAuto = 3;
                }
                else if (name.StartsWith("IC "))
                {
                    catalog = "IC";
                    prioAuto = 4;
                }
                else if (name.StartsWith("SH "))
                {
                    catalog = "SHARPLESS";
                    prioAuto = 5;
                }
                else if (name.StartsWith("UGC "))
                {
                    catalog = "UGC";
                    prioAuto = 6;
                }
                else if (name.StartsWith("UGCG "))
                {
                    catalog = "UGCG";
                    prioAuto = 7;
                }
                else if (name.StartsWith("PGC "))
                {
                    catalog = "PGC";
                    prioAuto = 8;
                }
                else if (name.StartsWith("LDN "))
                {
                    catalog = "LDN";
                    prioAuto = 9;
                }
                else if (name.StartsWith("LBN "))
                {
                    catalog = "LBN";
                    prioAuto = 10;
                }
                else if (name.StartsWith("ACO "))
                {
                    catalog = "ACO";
                    prioAuto = 11;
                }
                else if (name.StartsWith("PN "))
                {
                    catalog = "PN";
                    prioAuto = 12;
                }
                else if (name.StartsWith("PK "))
                {
                    catalog = "PK";
                    prioAuto = 13;
                }
                else if (name.StartsWith("LEDA "))
                {
                    catalog = "LEDA";
                    prioAuto = 14;
                }
                else if (name.StartsWith("SNR "))
                {
                    catalog = "SNR";
                    prioAuto = 15;
                }
                else if (name.StartsWith("Barnard "))
                {
                    catalog = "BARNARD";
                    prioAuto = 16;
                }
                else if (name.Contains("Melotte"))
                {
                    catalog = "MELOTTE";
                    prioAuto = 17;
                }
                else if (name.Contains("Collinder"))
                {
                    catalog = "COLLINDER";
                    prioAuto = 18;
                }
                else if (name.StartsWith("C "))
                {
                    catalog = "CALDWELL";
                    prioAuto = 19;
                }

                Names.Add(new ObjectNameDto
                {
                    Name = name.Replace("NAME", "").Trim(),
                    Priority = prioAuto,
                    Catalog = catalog
                });
            }
        }


        /// <summary>
        /// Gets the Telescopius URL for this deep sky object.
        /// Example: https://telescopius.com/deep-sky-objects/m-81
        /// </summary>
        /// <returns>The Telescopius URL for this object, or null if no valid identifier is found</returns>
        public string? GetTelescopiusUrl()
        {
            if (string.IsNullOrWhiteSpace(Identifier))
                return null;

            // Clean the identifier and convert to lowercase for URL
            string cleanId = Identifier.Trim().ToLowerInvariant();

            // Handle different catalog formats
            if (cleanId.StartsWith("m "))
            {
                cleanId = "m-" + cleanId[2..].TrimStart();
            }
            else if (cleanId.StartsWith("ngc "))
            {
                cleanId = "ngc-" + cleanId[4..].TrimStart();
            }
            else if (cleanId.StartsWith("ic "))
            {
                cleanId = "ic-" + cleanId[3..].TrimStart();
            }
            else if (cleanId.StartsWith("sh2-") || cleanId.StartsWith("ldn ") || cleanId.StartsWith("lbn "))
            {
                // Replace space with dash for these catalogs
                cleanId = cleanId.Replace(" ", "-");
            }
            else if (cleanId.StartsWith("abell "))
            {
                cleanId = "abell-" + cleanId[6..].TrimStart();
            }
            else if (cleanId.StartsWith("pk "))
            {
                cleanId = "pk-" + cleanId[3..].TrimStart()
                    .Replace("+", "plus-")
                    .Replace("-", "minus-");
            }

            // Remove any remaining spaces
            cleanId = cleanId.Replace(" ", "-");

            cleanId = cleanId.Replace("--", "-");

            // Build the URL
            return $"https://telescopius.com/deep-sky-objects/{cleanId}";
        }


        public string GetTypeDisplayName()
        {
            if (SimbadObjectType == null)
            {
                return "";
            }
            else
            {
                string typeName = SimbadObjectType.BaseType;

                if (!string.IsNullOrEmpty(MorphType))
                {
                    typeName = $"{typeName} ({MorphType})";
                }

                return typeName;
            }
        }


        public string GetFormattedMagnitudes()
        {
            string mag = "Unknown";

            string magV = MagnitudeV != null && MagnitudeV != 0 ? MagnitudeV.ToString() : "";
            string magP = MagnitudeP != null && MagnitudeP != 0 ? MagnitudeP.ToString() : "";

            mag = $"V: {magV} / P: {magP}";

            return mag;
        }



        public string GetUrlAstrobin()
        {
            var searchQuery = !string.IsNullOrEmpty(PrimaryName) ? PrimaryName : Identifier;
            return $"https://www.astrobin.com/search/?q={Uri.EscapeDataString(searchQuery)}";
        }

    }
}

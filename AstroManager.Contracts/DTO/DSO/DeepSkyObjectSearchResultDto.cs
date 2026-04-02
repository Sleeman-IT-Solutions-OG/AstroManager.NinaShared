using Shared.Model.DTO.Images;

namespace Shared.Model.DTO.DSO
{
    public class DeepSkyObjectSearchResultDto
    {
        public DeepSkyObjectDto Object { get; set; }

        public string MatchingName { get; set; }
        public int MatchPriority { get; set; }

        public int? ObservableMinutes { get; set; } = null;
        
        /// <summary>
        /// Optional image data for the DSO. Only populated when IncludeImageData is true in the search filter.
        /// </summary>
        public DeepSkyObjectImageDto? ImageData { get; set; }
        
        /// <summary>
        /// Next opposition date for the DSO. Calculated during search to avoid individual API calls.
        /// </summary>
        public DateTime? NextOppositionDate { get; set; }
    }
}

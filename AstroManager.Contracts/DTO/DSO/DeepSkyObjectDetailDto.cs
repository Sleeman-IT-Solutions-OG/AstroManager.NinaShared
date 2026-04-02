using Shared.Model.DTO.Astro;
using Shared.Model.DTO.Images;

namespace Shared.Model.DTO.DSO
{
    public class DeepSkyObjectDetailDto
    {
        public DeepSkyObjectDto DSO { get; set; }

        /// <summary>
        /// Optional image data for the DSO. Only populated when IncludeImageData is true in the search filter.
        /// </summary>
        public DeepSkyObjectImageDto? ImageData { get; set; }
        
        /// <summary>
        /// Next opposition date for the DSO. Calculated during search to avoid individual API calls.
        /// </summary>
        public DateTime? NextOppositionDate { get; set; }


        public BatchChartDataResponseDto ChartData { get; set; } = new BatchChartDataResponseDto();
    }
}

using System;

namespace Shared.Model.DTO.Images
{
    /// <summary>
    /// Data Transfer Object for deep sky object image data
    /// </summary>
    public class DeepSkyObjectImageDto
    {
        public Guid Id { get; set; }
        public Guid DeepSkyObjectId { get; set; }
        
        /// <summary>
        /// Base64 encoded image data
        /// </summary>
        public string ImageDataBase64 { get; set; } = string.Empty;
        
        public string Format { get; set; } = "jpg";
        public string ContentType { get; set; } = "image/jpeg";
        public int Width { get; set; }
        public int Height { get; set; }
        public double FieldOfView { get; set; }
        public string? SourceUrl { get; set; }
        public string? HipsSurvey { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.Master
{
    public class SimbadObjectTypeDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string OType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Label { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public bool IsCandidate { get; set; }

        [MaxLength(200)]
        public string? Path { get; set; }

        public string BaseType { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}

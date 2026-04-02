using System.ComponentModel.DataAnnotations;

namespace Shared.Model.DTO.DSO
{
    public class ObjectNameDto
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Priority { get; set; }

        public string? Catalog { get; set; }

        public Guid DeepSkyObjectId { get; set; }
    }
}

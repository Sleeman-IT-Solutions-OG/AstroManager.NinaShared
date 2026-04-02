using Shared.Model.DTO.Settings;

namespace Shared.Model.DTO.DSO
{
    public class ObservationNoteDto
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// The type of entity this note is attached to (e.g., "DeepSkyObject", "TargetListItem")
        /// </summary>
        public string EntityType { get; set; } = "DeepSkyObject";
        
        /// <summary>
        /// The ID of the entity this note is attached to
        /// </summary>
        public Guid EntityId { get; set; }
        
        /// <summary>
        /// Legacy property for backward compatibility - maps to EntityId when EntityType is "DeepSkyObject"
        /// </summary>
        public Guid DeepSkyObjectId 
        { 
            get => EntityType == "DeepSkyObject" ? EntityId : Guid.Empty;
            set { if (EntityType == "DeepSkyObject") EntityId = value; }
        }
        
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public List<ECameraFilter> InterestedFilters { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateObservationNoteDto
    {
        /// <summary>
        /// The type of entity this note is attached to (e.g., "DeepSkyObject", "TargetListItem")
        /// </summary>
        public string EntityType { get; set; } = "DeepSkyObject";
        
        /// <summary>
        /// The ID of the entity this note is attached to
        /// </summary>
        public Guid EntityId { get; set; }
        
        /// <summary>
        /// Legacy property for backward compatibility - maps to EntityId when EntityType is "DeepSkyObject"
        /// </summary>
        public Guid DeepSkyObjectId 
        { 
            get => EntityType == "DeepSkyObject" ? EntityId : Guid.Empty;
            set { if (EntityType == "DeepSkyObject") EntityId = value; }
        }
        
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public List<ECameraFilter> InterestedFilters { get; set; } = new();
    }

    public class UpdateObservationNoteDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public List<ECameraFilter> InterestedFilters { get; set; } = new();
    }
}

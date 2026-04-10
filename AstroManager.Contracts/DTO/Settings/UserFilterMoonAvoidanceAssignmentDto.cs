namespace Shared.Model.DTO.Settings
{
    public class UserFilterMoonAvoidanceAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? FilterDefinitionId { get; set; }
        public UserFilterDefinitionDto? FilterDefinition { get; set; }
        public string FilterName { get; set; } = string.Empty;
        public ECameraFilter? StandardFilter { get; set; }
        public Guid MoonAvoidanceProfileId { get; set; }
        public MoonAvoidanceProfileDto? MoonAvoidanceProfile { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}

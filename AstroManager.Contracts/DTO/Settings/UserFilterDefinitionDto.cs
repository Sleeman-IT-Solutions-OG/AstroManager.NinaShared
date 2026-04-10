namespace Shared.Model.DTO.Settings
{
    public class UserFilterDefinitionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ECameraFilter? StandardFilter { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}

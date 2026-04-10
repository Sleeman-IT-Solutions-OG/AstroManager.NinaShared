namespace Shared.Model.DTO.Settings
{
    public class EquipmentFilterAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid? FilterDefinitionId { get; set; }
        public UserFilterDefinitionDto? FilterDefinition { get; set; }
        public string FilterName { get; set; } = string.Empty;
        public ECameraFilter? StandardFilter { get; set; }
        public int? DefaultExposureTimeSeconds { get; set; }
        public int SortOrder { get; set; }
    }
}

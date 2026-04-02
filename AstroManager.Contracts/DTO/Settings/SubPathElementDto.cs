namespace Shared.Model.DTO.Settings
{
    public class SubPathElementDto
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string PathSegment { get; set; }
        public Guid WorkingDirectoryId { get; set; }
    }
}

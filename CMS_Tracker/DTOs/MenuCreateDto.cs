namespace CMS_Tracker.DTOs
{
    public class MenuCreateDto
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public Guid? ParentId { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
        public string Type { get; set; } = "page";
    }
}

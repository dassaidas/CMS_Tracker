namespace CMS_Tracker.DTOs
{
    public class MenuDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public string Type { get; set; }
        public int OrderIndex { get; set; }
        public Guid? ParentId { get; set; }
        public bool? IsActive { get; set; }

        public List<MenuDto> Children { get; set; } = new();
    }
}

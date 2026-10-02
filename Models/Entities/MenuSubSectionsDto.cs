namespace SWWebAPI.Models.Entities
{
    public class MenuSubSectionsDto
    {
        public string? MenuName { get; set; }

        public string? SectionTitle { get; set; }

        public string? SectionName { get; set; }

        public string? CoverImage { get; set; }

        public string? Title { get; set; }

        public string? Text { get; set; }

        public string? Image { get; set; }
    }
    public class SwatiMenuDto
    {
        public string? MenuName { get; set; }

        public List<SwatiSectionDto> Sections { get; set; } = new();
    }

    public class SwatiSectionDto
    {
        public string? SectionTitle { get; set; }

        public string? SectionName { get; set; }

        public string? CoverImage { get; set; }

        public List<SwatiTileDto> Tiles { get; set; } = new();
    }

    public class SwatiTileDto
    {
        public string? Title { get; set; }

        public string? Text { get; set; }

        public string? Image { get; set; }
    }
}

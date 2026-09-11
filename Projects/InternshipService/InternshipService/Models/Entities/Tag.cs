namespace InternshipService.Models.Entities;

public enum TagCategory
{
    Technology,
    Business,
    Design
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TagCategory Category { get; set; }
    
    public ICollection<VacancyTag> VacancyTags { get; set; } = new List<VacancyTag>();
}


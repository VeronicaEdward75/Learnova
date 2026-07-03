using LearnNova.Models.Enums;

namespace LearnNova.Models.Entities;

public class CourseContent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public ContentType Type { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public int? DurationSec { get; set; }
    public int OrderIndex { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public ICollection<ContentProgress> ProgressRecords { get; set; } = new List<ContentProgress>();
}

using LearnNova.Models.Entities;

namespace LearnNova.Models.ViewModels.Student;

public class LearningViewModel
{
    public Course Course { get; set; } = null!;
    public List<CourseContent> Contents { get; set; } = new();
    public CourseContent? CurrentContent { get; set; }
    
    public int? NextContentId { get; set; }
    public int? PrevContentId { get; set; }
    
    public List<int> CompletedContentIds { get; set; } = new();
    public int CourseProgressPercentage { get; set; }
}

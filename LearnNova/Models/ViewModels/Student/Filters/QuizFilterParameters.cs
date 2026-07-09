namespace LearnNova.Models.ViewModels.Student.Filters;

public class QuizFilterParameters : BaseFilterParameters
{
    public string? Status { get; set; }
    
    public override string? GetStatus() => Status;
}

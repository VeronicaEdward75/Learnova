namespace LearnNova.Models.ViewModels.Student.Filters;

public class AssignmentFilterParameters : BaseFilterParameters
{
    public string? Status { get; set; }
    
    public override string? GetStatus() => Status;
}

namespace LearnNova.Models.ViewModels.Student.Filters;

public class RefundFilterParameters : BaseFilterParameters
{
    public string? Status { get; set; }
    
    public override string? GetStatus() => Status;
}

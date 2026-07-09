namespace LearnNova.Models.ViewModels.Student.Filters;

public class OrderFilterParameters : BaseFilterParameters
{
    public string? Status { get; set; }
    public bool? HasInvoice { get; set; }
    
    public override string? GetStatus() => Status;
}

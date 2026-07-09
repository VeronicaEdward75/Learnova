namespace LearnNova.Models.ViewModels.Student.Filters;

public class WalletFilterParameters : BaseFilterParameters
{
    public string? Type { get; set; }
    
    public override string? GetStatus() => Type;
}

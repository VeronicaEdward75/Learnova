using System;
using LearnNova.Models.ViewModels.Student.Filters;

namespace LearnNova.Models.ViewModels.Teacher.Filters;

public class TeacherWalletFilterParameters : BaseFilterParameters
{
    public string? Type { get; set; }
    public string? Status { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? DateRange { get; set; }

    public override string? GetStatus() => Status;
}

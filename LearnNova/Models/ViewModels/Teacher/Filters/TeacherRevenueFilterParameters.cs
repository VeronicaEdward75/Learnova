using System;
using LearnNova.Models.ViewModels.Student.Filters;

namespace LearnNova.Models.ViewModels.Teacher.Filters;

public class TeacherRevenueFilterParameters : BaseFilterParameters
{
    public int? CourseId { get; set; }
    public string? Status { get; set; }
    public string? DateRange { get; set; }

    public override string? GetStatus() => Status;
}

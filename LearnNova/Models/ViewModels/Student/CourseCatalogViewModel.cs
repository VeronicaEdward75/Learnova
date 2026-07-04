using LearnNova.Models.Entities;
using LearnNova.Models.Lookups;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearnNova.Models.ViewModels.Student;

/// <summary>
/// Wraps the published courses list together with the current filter state
/// so the view can re-populate the search form without extra ViewBag entries.
/// </summary>
public class CourseCatalogViewModel
{
    public IEnumerable<Course> Courses { get; set; } = Enumerable.Empty<Course>();

    // Current filter values (round-trip to view)
    public string? Term    { get; set; }
    public string? Subject { get; set; }
    public string? Stage   { get; set; }

    // Select list sources
    public List<SelectListItem> SubjectOptions { get; set; } = BuildSubjectList();
    public List<SelectListItem> StageOptions   { get; set; } = BuildStageList();

    private static List<SelectListItem> BuildSubjectList() =>
        CurriculumLookup.Subjects
            .Select(kvp => new SelectListItem(kvp.Value, kvp.Key))
            .ToList();

    private static List<SelectListItem> BuildStageList() =>
        CurriculumLookup.Stages
            .Select(s => new SelectListItem(s, s))
            .ToList();
}

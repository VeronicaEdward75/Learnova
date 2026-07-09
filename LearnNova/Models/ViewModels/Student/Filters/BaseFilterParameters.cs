using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearnNova.Models.ViewModels.Student.Filters;

public class BaseFilterParameters
{
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // These are populated by the controller for the view to render dropdowns
    public List<SelectListItem> SortOptions { get; set; } = new();
    public List<SelectListItem> StatusOptions { get; set; } = new();
    
    // Derived classes can override this to return their specific status property
    public virtual string? GetStatus() => null;
}

using System.ComponentModel.DataAnnotations;
using LearnNova.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearnNova.Models.ViewModels.Teacher;

public class CourseContentFormViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "يرجى إدخال عنوان المحتوى")]
    [Display(Name = "العنوان")]
    public string Title { get; set; } = string.Empty;
    

    
    [Display(Name = "نوع المصدر")]
    public string SourceType { get; set; } = "URL";

    [Display(Name = "الرابط (فيديو أو ملف خارجي)")]
    public string FileUrl { get; set; } = string.Empty;

    [Display(Name = "رفع ملف")]
    public IFormFile? UploadFile { get; set; }
    
    [Display(Name = "المدة (بالثواني)")]
    public int? DurationSec { get; set; }
    
    public int CourseId { get; set; }
    
}

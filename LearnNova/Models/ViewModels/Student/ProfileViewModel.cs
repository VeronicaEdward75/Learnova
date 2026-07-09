using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace LearnNova.Models.ViewModels.Student;

public class ProfileViewModel
{
    // Display Fields
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? StudentId { get; set; }
    
    [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
    [Display(Name = "الاسم بالكامل")]
    public string FullName { get; set; } = string.Empty;
    
    [Display(Name = "رقم الهاتف")]
    [Phone(ErrorMessage = "رقم هاتف غير صالح")]
    public string? PhoneNumber { get; set; }
    
    [Display(Name = "الجنس")]
    public string? Gender { get; set; }
    
    [Display(Name = "تاريخ الميلاد")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
    
    [Display(Name = "الدولة")]
    public string? Country { get; set; }
    
    [Display(Name = "المدينة")]
    public string? City { get; set; }
    
    [Display(Name = "نبذة تعريفية")]
    [StringLength(500, ErrorMessage = "النبذة يجب ألا تتجاوز 500 حرف")]
    public string? Biography { get; set; }
    
    public string? ProfileImagePath { get; set; }
    
    [Display(Name = "الصورة الشخصية")]
    public IFormFile? ProfilePicture { get; set; }
    
    // Statistics
    public DateTime RegistrationDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int EnrolledCourses { get; set; }
    public int CompletedCourses { get; set; }
    public int CertificatesEarned { get; set; }
    public decimal WalletBalance { get; set; }
    
    // Teacher Statistics
    public int PublishedCourses { get; set; }
    public int TotalStudents { get; set; }
    public decimal TotalEarnings { get; set; }
    
    // Admin Statistics
    public int TotalUsers { get; set; }
    public int TotalPlatformCourses { get; set; }
    public decimal PlatformRevenue { get; set; }
}

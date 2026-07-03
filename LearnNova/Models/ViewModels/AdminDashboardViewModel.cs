namespace LearnNova.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalAdmins { get; set; }
    public int PendingTeachersCount { get; set; }
    public int TotalCourses { get; set; }
    public int PublishedCoursesCount { get; set; }
}

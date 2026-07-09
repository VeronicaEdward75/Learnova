using LearnNova.Models.ViewModels.Student;

namespace LearnNova.Services;

public interface IStudentDashboardService
{
    Task<StudentDashboardViewModel> GetDashboardAsync(string studentId, string studentName);
}

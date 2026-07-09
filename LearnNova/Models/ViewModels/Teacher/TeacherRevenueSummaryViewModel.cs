namespace LearnNova.Models.ViewModels.Teacher;

public class TeacherRevenueSummaryViewModel
{
    public decimal PendingBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal TotalEarnings { get; set; }
    public int TotalCoursesSold { get; set; }
    public decimal MonthlyRevenue { get; set; }
}

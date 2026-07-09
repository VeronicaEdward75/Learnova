using System;
using System.Collections.Generic;

namespace LearnNova.Models.ViewModels.Analytics;

public class TeacherFinanceDashboardViewModel
{
    // Financials
    public decimal TodayRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal TotalWithdrawn { get; set; }

    // Metrics
    public int StudentsCount { get; set; }
    public int CoursesSold { get; set; }
    public double AverageRating { get; set; }
    public double CompletionRate { get; set; }

    // General counts
    public int TotalCourses { get; set; }
    public int PublishedCount { get; set; }
    public int UnpublishedCount { get; set; }

    // Course Analytics Table
    public List<CourseAnalyticsViewModel> CourseAnalytics { get; set; } = new();
}

public class CourseAnalyticsViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Sales { get; set; }
    public decimal Revenue { get; set; }
    public int Students { get; set; }
    public double CompletionRate { get; set; }
    public double AverageRating { get; set; }
    public double QuizAverage { get; set; }
    public double AssignmentCompletion { get; set; }
}

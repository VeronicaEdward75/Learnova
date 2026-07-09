using System;
using System.Collections.Generic;

namespace LearnNova.Models.ViewModels.Analytics;

public class AdminFinanceDashboardViewModel
{
    public decimal TotalRevenue { get; set; }
    public decimal PlatformRevenue { get; set; }
    public decimal TeacherRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeeklyRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public decimal RefundedAmount { get; set; }
    public double RefundRate { get; set; }
    public List<TopCourseViewModel> TopRefundedCourses { get; set; } = new();
    public List<TopTeacherViewModel> TopRefundedTeachers { get; set; } = new();

    public int WithdrawalRequests { get; set; }
    public int ApprovedWithdrawals { get; set; }
    public int RejectedWithdrawals { get; set; }

    public decimal AverageOrderValue { get; set; }
    public int NumberOfSales { get; set; }
    public int NewCustomers { get; set; }
    public int RepeatCustomers { get; set; }

    public List<TopCourseViewModel> TopSellingCourses { get; set; } = new();
    public List<TopCourseViewModel> TopRevenueCourses { get; set; } = new();
    public List<TopCourseViewModel> HighestRatedCourses { get; set; } = new();
    public List<TopCourseViewModel> MostEnrolledCourses { get; set; } = new();

    public List<TopTeacherViewModel> HighestRevenueTeachers { get; set; } = new();
    public List<TopTeacherViewModel> MostSalesTeachers { get; set; } = new();
    public List<TopTeacherViewModel> MostStudentsTeachers { get; set; } = new();
    public List<TopTeacherViewModel> HighestRatedTeachers { get; set; } = new();

    public string DateFilter { get; set; } = "Last 30 Days";
    
    // Chart Data
    public List<string> ChartLabels { get; set; } = new();
    public List<decimal> RevenueChartData { get; set; } = new();
    public List<int> SalesChartData { get; set; } = new();
    public List<decimal> TeacherRevenueChartData { get; set; } = new();
    public List<decimal> PlatformRevenueChartData { get; set; } = new();
    public List<int> EnrollmentGrowthData { get; set; } = new();
}

public class TopCourseViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Sales { get; set; }
    public int Students { get; set; }
    public double AverageRating { get; set; }
}

public class TopTeacherViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Sales { get; set; }
    public int Students { get; set; }
    public double AverageRating { get; set; }
}


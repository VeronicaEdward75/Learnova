using System;
using System.Collections.Generic;

namespace LearnNova.Models.ViewModels.Analytics;

public class TeacherRevenuePageViewModel
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal AvailableBalance { get; set; }

    public string DateFilter { get; set; } = "This Month";

    // Charts
    public List<string> ChartLabels { get; set; } = new();
    public List<decimal> RevenueChartData { get; set; } = new();
    public List<int> SalesChartData { get; set; } = new();

    public List<TeacherSaleViewModel> RecentSales { get; set; } = new();
}

public class TeacherSaleViewModel
{
    public int PaymentId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

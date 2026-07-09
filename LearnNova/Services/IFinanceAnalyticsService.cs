using System;
using System.Threading.Tasks;
using LearnNova.Models.ViewModels.Analytics;

namespace LearnNova.Services;

public interface IFinanceAnalyticsService
{
    Task<AdminFinanceDashboardViewModel> GetAdminFinanceDashboardAsync(string dateFilter);
    Task<TeacherFinanceDashboardViewModel> GetTeacherFinanceDashboardAsync(string teacherId);
    Task<TeacherRevenuePageViewModel> GetTeacherRevenuePageAsync(string teacherId, string dateFilter);
    
    // Natively returns CSV bytes or string
    Task<string> GenerateRevenueReportCsvAsync(string dateFilter);
    Task<string> GenerateTeacherReportCsvAsync(string dateFilter);
}

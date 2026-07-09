using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Analytics;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class FinanceAnalyticsService : IFinanceAnalyticsService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IGenericRepository<WithdrawalRequest> _withdrawalRepository;
    private readonly IWalletService _walletService;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserService _userService;

    public FinanceAnalyticsService(
        IPaymentRepository paymentRepository,
        IGenericRepository<WithdrawalRequest> withdrawalRepository,
        IWalletService walletService,
        ICourseRepository courseRepository,
        IUserService userService)
    {
        _paymentRepository = paymentRepository;
        _withdrawalRepository = withdrawalRepository;
        _walletService = walletService;
        _courseRepository = courseRepository;
        _userService = userService;
    }

    private (DateTime Start, DateTime End) ParseDateFilter(string filter)
    {
        var now = DateTime.UtcNow;
        return filter switch
        {
            "Today" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
            "Last 7 Days" => (now.Date.AddDays(-7), now),
            "This Month" => (new DateTime(now.Year, now.Month, 1), now),
            "Last 30 Days" => (now.AddDays(-30), now),
            "This Year" => (new DateTime(now.Year, 1, 1), now),
            _ => (now.AddDays(-30), now) // Default to Last 30 Days
        };
    }

    public async Task<AdminFinanceDashboardViewModel> GetAdminFinanceDashboardAsync(string dateFilter)
    {
        var (start, end) = ParseDateFilter(dateFilter);
        var allPayments = await _paymentRepository.GetAllAsync();
        var allSucceeded = allPayments.Where(p => p.Status == PaymentStatus.Succeeded || p.Status == PaymentStatus.Pending).ToList();
        var allPending = allPayments.Where(p => p.Status == PaymentStatus.Pending).ToList();
        var allRefunded = allPayments.Where(p => p.Status == PaymentStatus.Refunded).ToList();

        var filteredPayments = allSucceeded.Where(p => p.CreatedAt >= start && p.CreatedAt <= end).ToList();

        var withdrawals = await _withdrawalRepository.GetAllAsync();

        var vm = new AdminFinanceDashboardViewModel
        {
            DateFilter = dateFilter,
            
            // Global metrics (ignores date filter for overall totals if needed, but standard is to filter some)
            TotalRevenue = allSucceeded.Sum(p => p.StudentPaid),
            PlatformRevenue = allSucceeded.Sum(p => p.PlatformFee),
            TeacherRevenue = allSucceeded.Sum(p => p.TeacherAmount),
            
            // Filtered metrics
            TodayRevenue = allSucceeded.Where(p => p.CreatedAt >= DateTime.UtcNow.Date).Sum(p => p.StudentPaid),
            WeeklyRevenue = allSucceeded.Where(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-7)).Sum(p => p.StudentPaid),
            MonthlyRevenue = allSucceeded.Where(p => p.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)).Sum(p => p.StudentPaid),
            YearlyRevenue = allSucceeded.Where(p => p.CreatedAt >= new DateTime(DateTime.UtcNow.Year, 1, 1)).Sum(p => p.StudentPaid),
            
            PendingRevenue = allPending.Sum(p => p.StudentPaid),
            RefundedAmount = allRefunded.Sum(p => p.StudentPaid),

            WithdrawalRequests = withdrawals.Count(),
            ApprovedWithdrawals = withdrawals.Count(w => w.Status == WithdrawalStatus.Approved),
            RejectedWithdrawals = withdrawals.Count(w => w.Status == WithdrawalStatus.Rejected),

            NumberOfSales = filteredPayments.Count,
            AverageOrderValue = filteredPayments.Count > 0 ? filteredPayments.Average(p => p.StudentPaid) : 0,
            
            NewCustomers = filteredPayments.Select(p => p.StudentId).Distinct().Count(),
            RepeatCustomers = 0 // Complex to calculate exactly without full history analysis per customer, simplified for now
        };

        // Top Courses
        vm.TopSellingCourses = filteredPayments.GroupBy(p => p.CourseId)
            .Select(g => new TopCourseViewModel
            {
                Id = g.Key,
                Title = g.First().Course?.Title ?? "Unknown",
                TeacherName = g.First().Course?.Teacher?.FullName ?? "Unknown",
                Revenue = g.Sum(p => p.StudentPaid),
                Sales = g.Count(),
                Students = g.Select(p => p.StudentId).Distinct().Count()
            })
            .OrderByDescending(c => c.Sales).Take(5).ToList();

        vm.TopRevenueCourses = vm.TopSellingCourses.OrderByDescending(c => c.Revenue).ToList();

        // Chart Data (Simplified logic for last 7 days)
        var dates = Enumerable.Range(0, 7).Select(i => DateTime.UtcNow.Date.AddDays(-6 + i)).ToList();
        vm.ChartLabels = dates.Select(d => d.ToString("MM/dd")).ToList();
        
        foreach (var d in dates)
        {
            var daySales = allSucceeded.Where(p => p.CreatedAt >= d && p.CreatedAt < d.AddDays(1)).ToList();
            vm.RevenueChartData.Add(daySales.Sum(p => p.StudentPaid));
            vm.SalesChartData.Add(daySales.Count);
            vm.PlatformRevenueChartData.Add(daySales.Sum(p => p.PlatformFee));
            vm.TeacherRevenueChartData.Add(daySales.Sum(p => p.TeacherAmount));
            vm.EnrollmentGrowthData.Add(daySales.Select(p => p.StudentId).Distinct().Count());
        }

        return vm;
    }

    public async Task<TeacherFinanceDashboardViewModel> GetTeacherFinanceDashboardAsync(string teacherId)
    {
        var allPayments = await _paymentRepository.GetAllAsync();
        var teacherPayments = allPayments.Where(p => p.Status == PaymentStatus.Succeeded && p.Course != null && p.Course.TeacherId == teacherId).ToList();

        var wallet = await _walletService.GetWalletAsync(teacherId);
        
        var courses = await _courseRepository.GetAllAsync();
        var teacherCourses = courses.Where(c => c.TeacherId == teacherId).ToList();

        var vm = new TeacherFinanceDashboardViewModel
        {
            TotalRevenue = wallet.TotalEarned,
            PendingBalance = wallet.PendingBalance,
            AvailableBalance = wallet.AvailableBalance,
            TotalWithdrawn = wallet.TotalWithdrawn,

            TodayRevenue = teacherPayments.Where(p => p.CreatedAt >= DateTime.UtcNow.Date).Sum(p => p.TeacherAmount),
            MonthlyRevenue = teacherPayments.Where(p => p.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)).Sum(p => p.TeacherAmount),

            StudentsCount = teacherPayments.Select(p => p.StudentId).Distinct().Count(),
            CoursesSold = teacherPayments.Count,
            AverageRating = 4.8, // Placeholder
            CompletionRate = 65.5, // Placeholder

            TotalCourses = teacherCourses.Count,
            PublishedCount = teacherCourses.Count(c => c.IsPublished),
            UnpublishedCount = teacherCourses.Count(c => !c.IsPublished)
        };

        foreach(var course in teacherCourses)
        {
            var courseSales = teacherPayments.Where(p => p.CourseId == course.Id).ToList();
            vm.CourseAnalytics.Add(new CourseAnalyticsViewModel
            {
                CourseId = course.Id,
                Title = course.Title,
                Sales = courseSales.Count,
                Revenue = courseSales.Sum(p => p.TeacherAmount),
                Students = courseSales.Select(p => p.StudentId).Distinct().Count(),
                CompletionRate = 0, // Need enrollment tracking for real
                AverageRating = 0,
                QuizAverage = 0,
                AssignmentCompletion = 0
            });
        }

        return vm;
    }

    public async Task<TeacherRevenuePageViewModel> GetTeacherRevenuePageAsync(string teacherId, string dateFilter)
    {
        var (start, end) = ParseDateFilter(dateFilter);
        var wallet = await _walletService.GetWalletAsync(teacherId);
        
        var allPayments = await _paymentRepository.GetAllAsync();
        var teacherSales = allPayments.Where(p => p.Status == PaymentStatus.Succeeded && p.Course != null && p.Course.TeacherId == teacherId).ToList();
        var filteredSales = teacherSales.Where(p => p.CreatedAt >= start && p.CreatedAt <= end).ToList();

        var vm = new TeacherRevenuePageViewModel
        {
            DateFilter = dateFilter,
            TotalRevenue = wallet.TotalEarned,
            MonthlyRevenue = teacherSales.Where(p => p.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)).Sum(p => p.TeacherAmount),
            PendingBalance = wallet.PendingBalance,
            AvailableBalance = wallet.AvailableBalance
        };

        vm.RecentSales = filteredSales.OrderByDescending(p => p.CreatedAt).Select(p => new TeacherSaleViewModel
        {
            PaymentId = p.Id,
            CourseTitle = p.Course?.Title ?? "N/A",
            StudentName = p.Student?.FullName ?? "N/A",
            Amount = p.TeacherAmount,
            Date = p.CreatedAt
        }).ToList();

        // Chart Data (7 Days)
        var dates = Enumerable.Range(0, 7).Select(i => DateTime.UtcNow.Date.AddDays(-6 + i)).ToList();
        vm.ChartLabels = dates.Select(d => d.ToString("MM/dd")).ToList();
        
        foreach (var d in dates)
        {
            var daySales = teacherSales.Where(p => p.CreatedAt >= d && p.CreatedAt < d.AddDays(1)).ToList();
            vm.RevenueChartData.Add(daySales.Sum(p => p.TeacherAmount));
            vm.SalesChartData.Add(daySales.Count);
        }

        return vm;
    }

    public async Task<string> GenerateRevenueReportCsvAsync(string dateFilter)
    {
        var (start, end) = ParseDateFilter(dateFilter);
        var allPayments = await _paymentRepository.GetAllAsync();
        var sales = allPayments.Where(p => p.Status == PaymentStatus.Succeeded && p.CreatedAt >= start && p.CreatedAt <= end).ToList();

        var csv = "Payment ID,Date,Course,Student,Amount,Platform Fee,Teacher Amount\n";
        foreach(var s in sales)
        {
            csv += $"{s.Id},{s.CreatedAt.ToString("yyyy-MM-dd HH:mm")},\"{s.Course?.Title}\",\"{s.Student?.FullName}\",{s.StudentPaid},{s.PlatformFee},{s.TeacherAmount}\n";
        }
        return csv;
    }

    public async Task<string> GenerateTeacherReportCsvAsync(string dateFilter)
    {
        var (start, end) = ParseDateFilter(dateFilter);
        var allPayments = await _paymentRepository.GetAllAsync();
        var sales = allPayments.Where(p => p.Status == PaymentStatus.Succeeded && p.CreatedAt >= start && p.CreatedAt <= end).ToList();

        var teacherGroup = sales.GroupBy(p => p.Course?.Teacher?.FullName ?? "Unknown");

        var csv = "Teacher,Total Sales,Total Revenue,Teacher Earnings,Platform Fee\n";
        foreach(var g in teacherGroup)
        {
            csv += $"\"{g.Key}\",{g.Count()},{g.Sum(p => p.StudentPaid)},{g.Sum(p => p.TeacherAmount)},{g.Sum(p => p.PlatformFee)}\n";
        }
        return csv;
    }
}




using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Analytics;
using LearnNova.Models.ViewModels.Admin;
using LearnNova.Repositories;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace LearnNova.Services;

public class FinanceAnalyticsService : IFinanceAnalyticsService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IGenericRepository<WithdrawalRequest> _withdrawalRepository;
    private readonly IWalletService _walletService;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserService _userService;
    private readonly LearnNova.Data.ApplicationDbContext _context;

    public FinanceAnalyticsService(
        IPaymentRepository paymentRepository,
        IGenericRepository<WithdrawalRequest> withdrawalRepository,
        IWalletService walletService,
        ICourseRepository courseRepository,
        IUserService userService,
        LearnNova.Data.ApplicationDbContext context)
    {
        _paymentRepository = paymentRepository;
        _withdrawalRepository = withdrawalRepository;
        _walletService = walletService;
        _courseRepository = courseRepository;
        _userService = userService;
        _context = context;
    }

    private (DateTime Start, DateTime End) ParseDateFilter(string filter, DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue) return (startDate.Value.Date, endDate.Value.Date.AddDays(1).AddTicks(-1));
        var now = DateTime.UtcNow;
        return filter switch
        {
            "Today" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
            "Last 7 Days" => (now.Date.AddDays(-7), now),
            "This Month" => (new DateTime(now.Year, now.Month, 1), now),
            "Last 30 Days" => (now.AddDays(-30), now),
            "This Year" => (new DateTime(now.Year, 1, 1), now),
            _ => (now.AddDays(-30), now)
        };
    }

    private IQueryable<Payment> ApplyAdminFilters(IQueryable<Payment> query, AdminFinanceFilterParameters filters, DateTime start, DateTime end)
    {
        query = query.Where(p => p.CreatedAt >= start && p.CreatedAt <= end);

        if (!string.IsNullOrWhiteSpace(filters.TeacherId))
            query = query.Where(p => p.Course != null && p.Course.TeacherId == filters.TeacherId);
        
        if (filters.CourseId.HasValue && filters.CourseId.Value > 0)
            query = query.Where(p => p.CourseId == filters.CourseId.Value);

        if (!string.IsNullOrWhiteSpace(filters.Subject))
            query = query.Where(p => p.Course != null && p.Course.Subject == filters.Subject);

        if (!string.IsNullOrWhiteSpace(filters.Stage))
            query = query.Where(p => p.Course != null && p.Course.Stage == filters.Stage);

        if (filters.PaymentStatus.HasValue)
            query = query.Where(p => p.Status == filters.PaymentStatus.Value);

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var search = filters.SearchTerm.ToLower();
            query = query.Where(p => (p.Course != null && p.Course.Title.ToLower().Contains(search)) 
                || (p.Student != null && p.Student.FullName.ToLower().Contains(search)));
        }

        return query;
    }

    public async Task<AdminFinanceDashboardViewModel> GetAdminFinanceDashboardAsync(AdminFinanceFilterParameters filters)
    {
        var (start, end) = ParseDateFilter(filters.DateFilter, filters.StartDate, filters.EndDate);
        
        var query = _context.Payments.AsNoTracking()
            .Include(p => p.Course).ThenInclude(c => c.Teacher)
            .Include(p => p.Student)
            .AsQueryable();

        var filteredQuery = ApplyAdminFilters(query, filters, start, end);
        var filteredList = await filteredQuery.ToListAsync();

        var allSucceeded = filteredList.Where(p => p.Status == PaymentStatus.Succeeded || p.Status == PaymentStatus.Pending).ToList();
        var allPending = filteredList.Where(p => p.Status == PaymentStatus.Pending).ToList();
        var allRefunded = filteredList.Where(p => p.Status == PaymentStatus.Refunded).ToList();

        var withdrawals = await _context.WithdrawalRequests.AsNoTracking().ToListAsync();

        var vm = new AdminFinanceDashboardViewModel
        {
            Filters = filters,
            
            TotalRevenue = allSucceeded.Sum(p => p.StudentPaid),
            PlatformRevenue = allSucceeded.Sum(p => p.PlatformFee),
            TeacherRevenue = allSucceeded.Sum(p => p.TeacherAmount),
            
            TodayRevenue = allSucceeded.Where(p => p.CreatedAt >= DateTime.UtcNow.Date).Sum(p => p.StudentPaid),
            WeeklyRevenue = allSucceeded.Where(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-7)).Sum(p => p.StudentPaid),
            MonthlyRevenue = allSucceeded.Where(p => p.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)).Sum(p => p.StudentPaid),
            YearlyRevenue = allSucceeded.Where(p => p.CreatedAt >= new DateTime(DateTime.UtcNow.Year, 1, 1)).Sum(p => p.StudentPaid),
            
            PendingRevenue = allPending.Sum(p => p.StudentPaid),
            RefundedAmount = allRefunded.Sum(p => p.StudentPaid),

            WithdrawalRequests = withdrawals.Count,
            ApprovedWithdrawals = withdrawals.Count(w => w.Status == WithdrawalStatus.Approved),
            RejectedWithdrawals = withdrawals.Count(w => w.Status == WithdrawalStatus.Rejected),

            NumberOfSales = filteredList.Count(p => p.Status == PaymentStatus.Succeeded),
            AverageOrderValue = allSucceeded.Count > 0 ? allSucceeded.Average(p => p.StudentPaid) : 0,
            
            NewCustomers = allSucceeded.Select(p => p.StudentId).Distinct().Count(),
        };

        var courseGroups = allSucceeded.GroupBy(p => p.Course).Where(g => g.Key != null).ToList();
        
        vm.TopSellingCourses = courseGroups.Select(g => new TopCourseViewModel
        {
            Id = g.Key.Id,
            Title = g.Key.Title ?? "Unknown",
            TeacherName = g.Key.Teacher?.FullName ?? "Unknown",
            Revenue = g.Sum(p => p.StudentPaid),
            Sales = g.Count(),
            Students = g.Select(p => p.StudentId).Distinct().Count(),
            RefundCount = allRefunded.Count(r => r.CourseId == g.Key.Id)
        }).OrderByDescending(c => c.Sales).Take(10).ToList();

        var topCourseIds = vm.TopSellingCourses.Select(c => c.Id).ToList();
        var compRates = await CalculateTeacherCoursesCompletionRatesAsync(topCourseIds);
        foreach (var c in vm.TopSellingCourses)
        {
            c.CompletionRate = compRates.GetValueOrDefault(c.Id, "-");
        }

        vm.TopRevenueCourses = vm.TopSellingCourses.OrderByDescending(c => c.Revenue).ToList();

        var teacherGroups = allSucceeded.GroupBy(p => p.Course?.Teacher).Where(g => g.Key != null).ToList();
        vm.HighestRevenueTeachers = teacherGroups.Select(g => new TopTeacherViewModel
        {
            Id = g.Key.Id,
            Name = g.Key.FullName ?? "Unknown",
            Revenue = g.Sum(p => p.TeacherAmount),
            Sales = g.Count(),
            Students = g.Select(p => p.StudentId).Distinct().Count(),
            Courses = g.Select(p => p.CourseId).Distinct().Count(),
            PendingWithdrawals = withdrawals.Count(w => w.UserId == g.Key.Id && w.Status == WithdrawalStatus.Pending)
        }).OrderByDescending(t => t.Revenue).Take(10).ToList();

        var totalDays = (end - start).Days;
        totalDays = totalDays == 0 ? 1 : totalDays + 1;
        if (totalDays > 31) totalDays = 31;
        for (int i = 0; i < totalDays; i++)
        {
            var d = start.AddDays(i).Date;
            var daySales = allSucceeded.Where(p => p.CreatedAt >= d && p.CreatedAt < d.AddDays(1)).ToList();
            vm.ChartLabels.Add(d.ToString("MM/dd"));
            vm.RevenueChartData.Add(daySales.Sum(p => p.StudentPaid));
            vm.SalesChartData.Add(daySales.Count);
            vm.PlatformRevenueChartData.Add(daySales.Sum(p => p.PlatformFee));
            vm.TeacherRevenueChartData.Add(daySales.Sum(p => p.TeacherAmount));
            
            var dayRefunds = allRefunded.Where(p => p.CreatedAt >= d && p.CreatedAt < d.AddDays(1)).ToList();
            vm.RefundTrendData.Add(dayRefunds.Count);
        }

        var subjectGroups = allSucceeded.GroupBy(p => string.IsNullOrWhiteSpace(p.Course?.Subject) ? "Other" : p.Course.Subject);
        foreach(var g in subjectGroups) {
            vm.SubjectChartLabels.Add(g.Key);
            vm.SubjectSalesData.Add(g.Count());
        }

        var stageGroups = allSucceeded.GroupBy(p => string.IsNullOrWhiteSpace(p.Course?.Stage) ? "Other" : p.Course.Stage);
        foreach(var g in stageGroups) {
            vm.StageChartLabels.Add(g.Key);
            vm.StageSalesData.Add(g.Count());
        }

        return vm;
    }

    private string CalculateOverallCompletion(Dictionary<int, string> completionRates)
    {
        var validRates = completionRates.Values.Where(v => v != "-").Select(v => double.Parse(v.TrimEnd('%'))).ToList();
        if (!validRates.Any()) return "-";
        return validRates.Average().ToString("0.0") + "%";
    }

    private async Task<Dictionary<int, string>> CalculateTeacherCoursesCompletionRatesAsync(List<int> courseIds)
    {
        var result = new Dictionary<int, string>();
        if (!courseIds.Any()) return result;

        var enrollments = await _context.Enrollments.AsNoTracking()
            .Where(e => courseIds.Contains(e.CourseId))
            .GroupBy(e => e.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var lessonsCount = await _context.CourseContents.AsNoTracking()
            .Where(c => courseIds.Contains(c.CourseId))
            .GroupBy(c => c.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var quizzesCount = await _context.Quizzes.AsNoTracking()
            .Where(q => courseIds.Contains(q.CourseId))
            .GroupBy(q => q.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var assignmentsCount = await _context.Assignments.AsNoTracking()
            .Where(a => courseIds.Contains(a.CourseId))
            .GroupBy(a => a.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var completedLessons = await _context.ContentProgressRecords.AsNoTracking()
            .Where(p => courseIds.Contains(p.Content.CourseId) && p.IsCompleted)
            .GroupBy(p => p.Content.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var completedQuizzes = await _context.QuizAttempts.AsNoTracking()
            .Where(qa => courseIds.Contains(qa.Quiz.CourseId))
            .Select(qa => new { qa.StudentId, qa.QuizId, qa.Quiz.CourseId })
            .Distinct()
            .GroupBy(x => x.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        var completedAssignments = await _context.AssignmentSubmissions.AsNoTracking()
            .Where(s => courseIds.Contains(s.Assignment.CourseId))
            .Select(s => new { s.StudentId, s.AssignmentId, s.Assignment.CourseId })
            .Distinct()
            .GroupBy(x => x.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Count);

        foreach (var cid in courseIds)
        {
            if (!enrollments.TryGetValue(cid, out int enrolledCount) || enrolledCount == 0)
            {
                result[cid] = "-";
                continue;
            }

            int tL = lessonsCount.GetValueOrDefault(cid);
            int tQ = quizzesCount.GetValueOrDefault(cid);
            int tA = assignmentsCount.GetValueOrDefault(cid);
            int totalComponents = tL + tQ + tA;

            if (totalComponents == 0)
            {
                result[cid] = "0.0%";
                continue;
            }

            int cL = completedLessons.GetValueOrDefault(cid);
            int cQ = completedQuizzes.GetValueOrDefault(cid);
            int cA = completedAssignments.GetValueOrDefault(cid);
            int totalCompleted = cL + cQ + cA;

            int maxPossibleCompletions = totalComponents * enrolledCount;
            double avg = ((double)totalCompleted / maxPossibleCompletions) * 100;
            result[cid] = avg.ToString("0.0") + "%";
        }

        return result;
    }

    public async Task<TeacherFinanceDashboardViewModel> GetTeacherFinanceDashboardAsync(string teacherId)
    {
        var wallet = await _walletService.GetWalletAsync(teacherId);
        
        var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var paymentsQuery = _paymentRepository.GetQueryable().AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Succeeded && p.Course != null && p.Course.TeacherId == teacherId);

        var teacherPayments = await paymentsQuery
            .Select(p => new { p.Id, p.CourseId, p.StudentId, p.TeacherAmount, p.CreatedAt, CourseTitle = p.Course.Title, StudentName = p.Student.FullName })
            .ToListAsync();

        var coursesQuery = _courseRepository.GetQueryable().AsNoTracking()
            .Where(c => c.TeacherId == teacherId);

        var teacherCourses = await coursesQuery
            .Select(c => new { c.Id, c.Title, c.IsPublished })
            .ToListAsync();

        var courseIds = teacherCourses.Select(c => c.Id).ToList();
        var completionRates = await CalculateTeacherCoursesCompletionRatesAsync(courseIds);

        var vm = new TeacherFinanceDashboardViewModel
        {
            TotalRevenue = wallet.TotalEarned,
            PendingBalance = wallet.PendingBalance,
            AvailableBalance = wallet.AvailableBalance,
            TotalWithdrawn = wallet.TotalWithdrawn,

            TodayRevenue = teacherPayments.Where(p => p.CreatedAt >= today).Sum(p => p.TeacherAmount),
            MonthlyRevenue = teacherPayments.Where(p => p.CreatedAt >= startOfMonth).Sum(p => p.TeacherAmount),

            StudentsCount = teacherPayments.Select(p => p.StudentId).Distinct().Count(),
            CoursesSold = teacherPayments.Count,
            CompletionRateDisplay = CalculateOverallCompletion(completionRates),

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
                CompletionRateDisplay = completionRates.GetValueOrDefault(course.Id, "-")
            });
        }

        vm.CourseAnalytics = vm.CourseAnalytics.OrderByDescending(c => c.Revenue).ToList();

        vm.RecentSales = teacherPayments.OrderByDescending(p => p.CreatedAt).Take(10).Select(p => new TeacherSaleViewModel
        {
            PaymentId = p.Id,
            CourseTitle = p.CourseTitle,
            StudentName = p.StudentName,
            Amount = p.TeacherAmount,
            Date = p.CreatedAt
        }).ToList();

        return vm;
    }

    public async Task<TeacherRevenuePageViewModel> GetTeacherRevenuePageAsync(string teacherId, LearnNova.Models.ViewModels.Teacher.Filters.TeacherRevenueFilterParameters filters)
    {
        var (start, end) = ParseDateFilter(filters.DateRange ?? "This Month", filters.StartDate, filters.EndDate);

        var wallet = await _walletService.GetWalletAsync(teacherId);

        var paymentsQuery = _paymentRepository.GetQueryable().AsNoTracking()
            .Where(p => p.Course != null && p.Course.TeacherId == teacherId);

        if (!string.IsNullOrWhiteSpace(filters.Status) && filters.Status != "All")
        {
            if (Enum.TryParse<PaymentStatus>(filters.Status, true, out var statusEnum))
            {
                paymentsQuery = paymentsQuery.Where(p => p.Status == statusEnum);
            }
        }
        else
        {
            // Default to successful if no filter or 'All' (wait, if 'All', maybe show all? The prompt says Status: All, Successful, Refunded, Pending. So we should include all if All. 
            // Previous code hardcoded p.Status == PaymentStatus.Succeeded. Let's make it filterable but default to Succeeded if not specified.
            if (string.IsNullOrWhiteSpace(filters.Status))
            {
                paymentsQuery = paymentsQuery.Where(p => p.Status == PaymentStatus.Succeeded);
            }
        }

        if (filters.CourseId.HasValue && filters.CourseId.Value > 0)
        {
            paymentsQuery = paymentsQuery.Where(p => p.CourseId == filters.CourseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var search = filters.SearchTerm.ToLower();
            paymentsQuery = paymentsQuery.Where(p => 
                p.Student != null && p.Student.FullName.ToLower().Contains(search)
            );
        }

        // Apply date filter
        paymentsQuery = paymentsQuery.Where(p => p.CreatedAt >= start && p.CreatedAt <= end);

        // Sorting
        paymentsQuery = filters.SortBy switch
        {
            "Oldest" => paymentsQuery.OrderBy(p => p.CreatedAt),
            "Highest Revenue" => paymentsQuery.OrderByDescending(p => p.TeacherAmount),
            "Most Sales" => paymentsQuery.OrderByDescending(p => p.CreatedAt), // doesn't make sense for individual payments to have most sales, sort by date as fallback
            _ => paymentsQuery.OrderByDescending(p => p.CreatedAt) // Newest
        };

        var filteredSales = await paymentsQuery
            .Select(p => new { p.Id, p.TeacherAmount, p.CreatedAt, CourseTitle = p.Course.Title, StudentName = p.Student.FullName })
            .ToListAsync();

        var vm = new TeacherRevenuePageViewModel
        {
            Filters = filters,
            TotalRevenue = wallet.TotalEarned,
            MonthlyRevenue = filteredSales.Sum(p => p.TeacherAmount),
            PendingBalance = wallet.PendingBalance,
            AvailableBalance = wallet.AvailableBalance
        };

        vm.RecentSales = filteredSales.Take(20).Select(p => new TeacherSaleViewModel
        {
            PaymentId = p.Id,
            CourseTitle = p.CourseTitle,
            StudentName = p.StudentName,
            Amount = p.TeacherAmount,
            Date = p.CreatedAt
        }).ToList();

        // Dynamic Chart Data based on DateFilter
        if ((filters.DateRange ?? "This Month") == "This Year")
        {
            for (int i = 1; i <= 12; i++)
            {
                var monthStart = new DateTime(start.Year, i, 1);
                var monthEnd = monthStart.AddMonths(1);
                var monthSales = filteredSales.Where(p => p.CreatedAt >= monthStart && p.CreatedAt < monthEnd).ToList();
                vm.ChartLabels.Add(monthStart.ToString("MMMM"));
                vm.RevenueChartData.Add(monthSales.Sum(p => p.TeacherAmount));
                vm.SalesChartData.Add(monthSales.Count);
            }
        }
        else
        {
            var totalDays = (end - start).Days;
            totalDays = totalDays == 0 ? 1 : totalDays + 1;

            if (totalDays > 31) totalDays = 31; // cap at 31 to avoid massive charts

            for (int i = 0; i < totalDays; i++)
            {
                var d = start.AddDays(i).Date;
                var daySales = filteredSales.Where(p => p.CreatedAt >= d && p.CreatedAt < d.AddDays(1)).ToList();
                vm.ChartLabels.Add(d.ToString("MM/dd"));
                vm.RevenueChartData.Add(daySales.Sum(p => p.TeacherAmount));
                vm.SalesChartData.Add(daySales.Count);
            }
        }

        return vm;
    }

    public async Task<string> GenerateRevenueReportCsvAsync(AdminFinanceFilterParameters filters)
    {
        var (start, end) = ParseDateFilter(filters.DateFilter, filters.StartDate, filters.EndDate);
        var query = _context.Payments.AsNoTracking().Include(p => p.Course).ThenInclude(c => c.Teacher).Include(p => p.Student).AsQueryable();
        query = ApplyAdminFilters(query, filters, start, end);
        var sales = await query.Where(p => p.Status == PaymentStatus.Succeeded).ToListAsync();

        var csv = "Payment ID,Date,Course,Teacher,Student,Amount,Platform Fee,Teacher Amount\n";
        foreach(var s in sales)
        {
            csv += $"{s.Id},{s.CreatedAt.ToString("yyyy-MM-dd HH:mm")},\"{s.Course?.Title}\",\"{s.Course?.Teacher?.FullName}\",\"{s.Student?.FullName}\",{s.StudentPaid},{s.PlatformFee},{s.TeacherAmount}\n";
        }
        return csv;
    }

    public async Task<byte[]> GenerateRevenueReportExcelAsync(AdminFinanceFilterParameters filters)
    {
        var (start, end) = ParseDateFilter(filters.DateFilter, filters.StartDate, filters.EndDate);
        var query = _context.Payments.AsNoTracking().Include(p => p.Course).ThenInclude(c => c.Teacher).Include(p => p.Student).AsQueryable();
        query = ApplyAdminFilters(query, filters, start, end);
        var sales = await query.Where(p => p.Status == PaymentStatus.Succeeded).ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Revenue Report");

        worksheet.Cell(1, 1).Value = "Payment ID";
        worksheet.Cell(1, 2).Value = "Date";
        worksheet.Cell(1, 3).Value = "Course";
        worksheet.Cell(1, 4).Value = "Teacher";
        worksheet.Cell(1, 5).Value = "Student";
        worksheet.Cell(1, 6).Value = "Amount Paid";
        worksheet.Cell(1, 7).Value = "Platform Fee";
        worksheet.Cell(1, 8).Value = "Teacher Amount";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach(var s in sales)
        {
            worksheet.Cell(row, 1).Value = s.Id;
            worksheet.Cell(row, 2).Value = s.CreatedAt;
            worksheet.Cell(row, 2).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
            worksheet.Cell(row, 3).Value = s.Course?.Title ?? "Unknown";
            worksheet.Cell(row, 4).Value = s.Course?.Teacher?.FullName ?? "Unknown";
            worksheet.Cell(row, 5).Value = s.Student?.FullName ?? "Unknown";
            worksheet.Cell(row, 6).Value = s.StudentPaid;
            worksheet.Cell(row, 7).Value = s.PlatformFee;
            worksheet.Cell(row, 8).Value = s.TeacherAmount;
            row++;
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GenerateRevenueReportPdfAsync(AdminFinanceFilterParameters filters)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var (start, end) = ParseDateFilter(filters.DateFilter, filters.StartDate, filters.EndDate);
        var query = _context.Payments.AsNoTracking().Include(p => p.Course).ThenInclude(c => c.Teacher).Include(p => p.Student).AsQueryable();
        query = ApplyAdminFilters(query, filters, start, end);
        var sales = await query.Where(p => p.Status == PaymentStatus.Succeeded).ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text("Revenue Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                
                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.ConstantColumn(60);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("#");
                        header.Cell().Text("Date");
                        header.Cell().Text("Course");
                        header.Cell().Text("Teacher");
                        header.Cell().Text("Amount");
                    });

                    foreach (var s in sales.Take(500))
                    {
                        table.Cell().Text(s.Id.ToString());
                        table.Cell().Text(s.CreatedAt.ToString("yyyy-MM-dd"));
                        table.Cell().Text(s.Course?.Title ?? "Unknown");
                        table.Cell().Text(s.Course?.Teacher?.FullName ?? "Unknown");
                        table.Cell().Text(s.StudentPaid.ToString("N2"));
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<string> GenerateTeacherReportCsvAsync(string dateFilter)
    {
        var (start, end) = ParseDateFilter(dateFilter, null, null);
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




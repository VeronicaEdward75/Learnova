using LearnNova.Repositories;
using LearnNova.Services;
using LearnNova.Services.DateTimeService;

namespace LearnNova.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ICourseContentRepository, CourseContentRepository>();
        services.AddScoped<IContentProgressRepository, ContentProgressRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<IAssignmentSubmissionRepository, AssignmentSubmissionRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
        services.AddScoped<IWithdrawalRequestRepository, WithdrawalRequestRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICouponUsageRepository, CouponUsageRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<ICourseContentService, CourseContentService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IAssignmentSubmissionService, AssignmentSubmissionService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IQuizQuestionService, QuizQuestionService>();
        services.AddScoped<IQuizAttemptService, QuizAttemptService>();
        services.AddScoped<IDateTimeService, SystemDateTimeService>();
        services.AddScoped<IEmailService, EmailService>();
        
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPaymentGateway, FakePaymentGateway>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IWithdrawalService, WithdrawalService>();
        services.AddScoped<IFinanceAnalyticsService, FinanceAnalyticsService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IRefundService, RefundService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<IStudentDashboardService, StudentDashboardService>();

        return services;
    }
}




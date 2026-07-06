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

        return services;
    }
}

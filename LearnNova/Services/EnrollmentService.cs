using LearnNova.Models.Entities;
using LearnNova.Repositories;
using LearnNova.Services.DateTimeService;

namespace LearnNova.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDateTimeService _dateTimeService;

    public EnrollmentService(IEnrollmentRepository enrollmentRepository, ICourseRepository courseRepository, IDateTimeService dateTimeService)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _dateTimeService = dateTimeService;
    }

    public async Task<ServiceResult> EnrollAsync(string studentId, int courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course is null || !course.IsPublished)
            return ServiceResult.Fail("الكورس غير موجود أو غير متاح للتسجيل.");

        var existing = await _enrollmentRepository.GetByStudentAndCourseAsync(studentId, courseId);
        if (existing is not null)
            return ServiceResult.Fail("أنت مسجل في هذا الكورس بالفعل.");

        var enrollment = new Enrollment
        {
            StudentId  = studentId,
            CourseId   = courseId,
            EnrolledAt = _dateTimeService.UtcNow(),
            IsActive   = true
        };

        await _enrollmentRepository.AddAsync(enrollment);
        await _enrollmentRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public Task<IEnumerable<Enrollment>> GetStudentCoursesAsync(string studentId) =>
        _enrollmentRepository.GetStudentEnrollmentsAsync(studentId);

    public Task<LearnNova.Models.ViewModels.PagedResult<Enrollment>> GetStudentCoursesPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.CourseFilterParameters filters) =>
        _enrollmentRepository.GetStudentEnrollmentsPagedAsync(studentId, filters);

    public async Task<bool> IsEnrolledAsync(string studentId, int courseId) =>
        await _enrollmentRepository.GetByStudentAndCourseAsync(studentId, courseId) is not null;

    public async Task<ServiceResult> UnenrollAsync(string studentId, int courseId)
    {
        var enrollment = (await _enrollmentRepository.FindAsync(e => e.StudentId == studentId && e.CourseId == courseId)).FirstOrDefault();
        if (enrollment == null) return ServiceResult.Fail("الطالب غير مسجل في الكورس.");
        
        _enrollmentRepository.Remove(enrollment);
        await _enrollmentRepository.SaveChangesAsync();
        
        return ServiceResult.Success();
    }
}


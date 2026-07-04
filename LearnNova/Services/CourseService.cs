using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Repositories;
using LearnNova.Services.DateTimeService;

namespace LearnNova.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IDateTimeService _dateTimeService;

    public CourseService(ICourseRepository courseRepository, IEnrollmentRepository enrollmentRepository, IDateTimeService dateTimeService)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _dateTimeService = dateTimeService;
    }

    // ─── Admin / Shared ───────────────────────────────────────────────────────

    public Task<IEnumerable<Course>> GetAllCoursesAsync() =>
        _courseRepository.GetAllWithTeacherAsync();

    public Task<IEnumerable<Course>> SearchCoursesAsync(string? term, bool? isPublished) =>
        _courseRepository.SearchAsync(term, isPublished);

    public Task<Course?> GetCourseByIdAsync(int id) =>
        _courseRepository.GetByIdAsync(id);

    /// <summary>
    /// Toggles publish status. When called from AdminController, requestingTeacherId is null
    /// (Admin can toggle any course). When called from TeacherController, we verify ownership first.
    /// </summary>
    public async Task<ServiceResult> SetPublishedStatusAsync(int id, bool isPublished, string? requestingTeacherId = null)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
            return ServiceResult.Fail("الكورس غير موجود");

        if (requestingTeacherId is not null && course.TeacherId != requestingTeacherId)
            return ServiceResult.Fail("غير مصرح لك بتعديل هذا الكورس");

        course.IsPublished = isPublished;
        _courseRepository.Update(course);
        await _courseRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }

    // ─── Teacher CRUD ─────────────────────────────────────────────────────────

    public Task<IEnumerable<Course>> GetTeacherCoursesAsync(string teacherId) =>
        _courseRepository.GetByTeacherIdAsync(teacherId);

    public async Task<ServiceResult> CreateCourseAsync(CourseFormViewModel model, string teacherId)
    {
        if (string.IsNullOrWhiteSpace(model.Title)) return ServiceResult.Fail("عنوان الكورس مطلوب.");
        if (string.IsNullOrWhiteSpace(model.Description)) return ServiceResult.Fail("وصف الكورس مطلوب.");
        if (model.Price < 0 || model.Price > 100000) return ServiceResult.Fail("السعر يجب أن يكون بين 0 و 100000.");
        
        var course = new Course
        {
            Title       = model.Title.Trim(),
            Description = model.Description?.Trim(),
            Subject     = model.Subject,
            Stage       = model.Stage,
            GradeLevel  = model.GradeLevel,
            Price       = model.Price,
            TeacherId   = teacherId,
            IsPublished = false,
            CreatedAt   = _dateTimeService.UtcNow()
        };

        await _courseRepository.AddAsync(course);
        await _courseRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateCourseAsync(int id, CourseFormViewModel model, string teacherId)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
            return ServiceResult.Fail("الكورس غير موجود");

        // Resource-based authorization: teacher can only edit their own course.
        if (course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بتعديل هذا الكورس");

        if (string.IsNullOrWhiteSpace(model.Title)) return ServiceResult.Fail("عنوان الكورس مطلوب.");
        if (string.IsNullOrWhiteSpace(model.Description)) return ServiceResult.Fail("وصف الكورس مطلوب.");
        if (model.Price < 0 || model.Price > 100000) return ServiceResult.Fail("السعر يجب أن يكون بين 0 و 100000.");


        course.Title       = model.Title.Trim();
        course.Description = model.Description?.Trim();
        course.Subject     = model.Subject;
        course.Stage       = model.Stage;
        course.GradeLevel  = model.GradeLevel;
        course.Price       = model.Price;

        _courseRepository.Update(course);
        await _courseRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteCourseAsync(int id, string teacherId)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
            return ServiceResult.Fail("الكورس غير موجود");

        // Resource-based authorization: teacher can only delete their own course.
        if (course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بحذف هذا الكورس");

        // Business Logic: Prevent deletion if there are active enrollments
        var enrollmentsCount = await _enrollmentRepository.GetCourseEnrollmentCountAsync(id);
        if (enrollmentsCount > 0)
            return ServiceResult.Fail("لا يمكن حذف الكورس لوجود طلاب مسجلين فيه. يمكنك إلغاء نشره بدلاً من ذلك.");

        _courseRepository.Remove(course);
        await _courseRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }

    // ─── Student Catalog ──────────────────────────────────────────────────────

    public Task<IEnumerable<Course>> SearchPublishedCoursesAsync(string? term, string? subject, string? stage) =>
        _courseRepository.SearchPublishedAsync(term, subject, stage);

    public Task<Course?> GetPublishedCourseByIdAsync(int id) =>
        _courseRepository.GetPublishedByIdAsync(id);
}

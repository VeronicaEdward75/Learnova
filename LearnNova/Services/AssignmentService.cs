using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Repositories;
using LearnNova.Services.DateTimeService;
using Microsoft.Extensions.Logging;

namespace LearnNova.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<AssignmentService> _logger;

    public AssignmentService(IAssignmentRepository assignmentRepository, ICourseRepository courseRepository, IDateTimeService dateTimeService, ILogger<AssignmentService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<IEnumerable<Assignment>> GetAssignmentsByCourseAsync(int courseId, string teacherId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course is null || course.TeacherId != teacherId)
            return Enumerable.Empty<Assignment>();

        return await _assignmentRepository.GetByCourseIdAsync(courseId);
    }

    public async Task<Assignment?> GetAssignmentByIdAsync(int id, string teacherId)
    {
        var assignment = await _assignmentRepository.GetByIdWithCourseAsync(id);
        if (assignment is null || assignment.Course.TeacherId != teacherId)
            return null;

        return assignment;
    }

    public async Task<ServiceResult> CreateAssignmentAsync(int courseId, AssignmentFormViewModel model, string teacherId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course is null || course.TeacherId != teacherId)
        {
            _logger.LogWarning("Security event: Teacher {TeacherId} attempted to create an assignment for course {CourseId} which they do not own.", teacherId, courseId);
            return ServiceResult.Fail("غير مصرح لك بإضافة مهمة لهذا الكورس.");
        }

        if (string.IsNullOrWhiteSpace(model.Title)) return ServiceResult.Fail("عنوان المهمة مطلوب.");
        if (string.IsNullOrWhiteSpace(model.Description)) return ServiceResult.Fail("وصف المهمة مطلوب.");
        if (model.MaxGrade <= 0) return ServiceResult.Fail("الدرجة القصوى يجب أن تكون أكبر من 0.");
        
        var dueUtc = _dateTimeService.ToUtc(model.DueDate);
        if (dueUtc <= _dateTimeService.UtcNow()) return ServiceResult.Fail("تاريخ الاستحقاق يجب أن يكون في المستقبل.");

        var assignment = new Assignment
        {
            CourseId = courseId,
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            DueDate = dueUtc,
            MaxGrade = model.MaxGrade
        };

        await _assignmentRepository.AddAsync(assignment);
        await _assignmentRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAssignmentAsync(int id, AssignmentFormViewModel model, string teacherId)
    {
        var assignment = await _assignmentRepository.GetByIdWithCourseAsync(id);
        if (assignment is null)
            return ServiceResult.Fail("المهمة غير موجودة.");

        if (assignment.Course.TeacherId != teacherId)
        {
            _logger.LogWarning("Security event: Teacher {TeacherId} attempted to update assignment {AssignmentId} which belongs to a different teacher.", teacherId, id);
            return ServiceResult.Fail("غير مصرح لك بتعديل هذه المهمة.");
        }

        if (string.IsNullOrWhiteSpace(model.Title)) return ServiceResult.Fail("عنوان المهمة مطلوب.");
        if (string.IsNullOrWhiteSpace(model.Description)) return ServiceResult.Fail("وصف المهمة مطلوب.");
        if (model.MaxGrade <= 0) return ServiceResult.Fail("الدرجة القصوى يجب أن تكون أكبر من 0.");
        
        var dueUtc = _dateTimeService.ToUtc(model.DueDate);
        
        // Only enforce future DueDate if the teacher is actually changing it to a new date
        if (dueUtc != assignment.DueDate && dueUtc <= _dateTimeService.UtcNow()) 
            return ServiceResult.Fail("تاريخ الاستحقاق الجديد يجب أن يكون في المستقبل.");


        assignment.Title = model.Title.Trim();
        assignment.Description = model.Description.Trim();
        assignment.DueDate = dueUtc;
        assignment.MaxGrade = model.MaxGrade;

        _assignmentRepository.Update(assignment);
        await _assignmentRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAssignmentAsync(int id, string teacherId)
    {
        var assignment = await _assignmentRepository.GetByIdWithCourseAsync(id);
        if (assignment is null)
            return ServiceResult.Fail("المهمة غير موجودة.");

        if (assignment.Course.TeacherId != teacherId)
        {
            _logger.LogWarning("Security event: Teacher {TeacherId} attempted to delete assignment {AssignmentId} which belongs to a different teacher.", teacherId, id);
            return ServiceResult.Fail("غير مصرح لك بحذف هذه المهمة.");
        }

        var submissionsCount = await _assignmentRepository.GetSubmissionsCountAsync(id);
        if (submissionsCount > 0)
            return ServiceResult.Fail("لا يمكن حذف هذه المهمة لوجود تسليمات من الطلاب. يمكنك إغلاقها بدلاً من ذلك.");

        _assignmentRepository.Remove(assignment);
        await _assignmentRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }
}

using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Repositories;
using LearnNova.Services.DateTimeService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;

namespace LearnNova.Services;

public class AssignmentSubmissionService : IAssignmentSubmissionService
{
    private readonly IAssignmentSubmissionRepository _submissionRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<AssignmentSubmissionService> _logger;

    public AssignmentSubmissionService(
        IAssignmentSubmissionRepository submissionRepository,
        IAssignmentRepository assignmentRepository,
        IEnrollmentRepository enrollmentRepository,
        ICourseRepository courseRepository,
        IDateTimeService dateTimeService,
        ILogger<AssignmentSubmissionService> logger)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<IEnumerable<AssignmentItemViewModel>> GetStudentAssignmentsWithStatusAsync(int courseId, string studentId)
    {
        var enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(studentId, courseId);
        if (enrollment == null)
            return Enumerable.Empty<AssignmentItemViewModel>();

        var assignments = await _assignmentRepository.GetByCourseIdAsync(courseId);
        var result = new List<AssignmentItemViewModel>();

        foreach (var assignment in assignments)
        {
            var submission = await _submissionRepository.GetSubmissionAsync(assignment.Id, studentId);
            result.Add(new AssignmentItemViewModel
            {
                Assignment = assignment,
                Submission = submission
            });
        }

        return result;
    }

    public async Task<AssignmentSubmitViewModel?> GetAssignmentForSubmissionAsync(int assignmentId, string studentId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
            return null;

        var enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(studentId, assignment.CourseId);
        if (enrollment == null)
            return null;

        var existingSubmission = await _submissionRepository.GetSubmissionAsync(assignmentId, studentId);

        return new AssignmentSubmitViewModel
        {
            Assignment = assignment,
            ExistingSubmission = existingSubmission
        };
    }

    public async Task<ServiceResult> SubmitAssignmentAsync(int assignmentId, string studentId, IFormFile? file, string? textAnswer, string webRootPath)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
            return ServiceResult.Fail("المهمة غير موجودة.");

        var enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(studentId, assignment.CourseId);
        if (enrollment == null)
            return ServiceResult.Fail("أنت غير مشترك في هذا الكورس.");

        if (_dateTimeService.UtcNow() > assignment.DueDate)
            return ServiceResult.Fail("لا يمكن تسليم المهمة بعد انتهاء الموعد المحدد.");


        var existingSubmission = await _submissionRepository.GetSubmissionAsync(assignmentId, studentId);
        if (existingSubmission != null)
            return ServiceResult.Fail("لقد قمت بتسليم هذه المهمة مسبقاً.");

        if (file == null && string.IsNullOrWhiteSpace(textAnswer))
            return ServiceResult.Fail("يجب إرفاق ملف أو كتابة إجابة نصية.");

        string? fileUrl = null;
        if (file != null && file.Length > 0)
        {
            var allowedExtensions = new[] { ".pdf", ".docx", ".zip" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Security event: Student {StudentId} attempted to upload an invalid file type {Extension} for assignment {AssignmentId}", studentId, extension, assignmentId);
                return ServiceResult.Fail("صيغة الملف غير مدعومة. الصيغ المسموحة: pdf, docx, zip");
            }

            if (file.Length > 20 * 1024 * 1024)
                return ServiceResult.Fail("حجم الملف يجب ألا يتجاوز 20 ميجابايت.");

            string uploadsFolder = Path.Combine(webRootPath, "uploads", "assignments");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            fileUrl = $"/uploads/assignments/{uniqueFileName}";
        }

        var isLate = _dateTimeService.UtcNow() > assignment.DueDate;

        var submission = new AssignmentSubmission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            FileUrl = fileUrl,
            TextAnswer = textAnswer,
            SubmittedAt = _dateTimeService.UtcNow(),
            Status = isLate ? SubmitStatus.Late : SubmitStatus.Submitted
        };

        await _submissionRepository.AddAsync(submission);
        await _submissionRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<IEnumerable<AssignmentSubmission>> GetSubmissionsForAssignmentAsync(int assignmentId, string teacherId)
    {
        var assignment = await _assignmentRepository.GetByIdWithCourseAsync(assignmentId);
        if (assignment == null || assignment.Course.TeacherId != teacherId)
            return Enumerable.Empty<AssignmentSubmission>();

        return await _submissionRepository.GetSubmissionsByAssignmentIdAsync(assignmentId);
    }

    public async Task<ServiceResult> GradeSubmissionAsync(SubmissionGradeViewModel model, string teacherId)
    {
        var submission = await _submissionRepository.GetSubmissionByIdWithAssignmentAsync(model.SubmissionId);
        if (submission == null)
            return ServiceResult.Fail("التسليم غير موجود.");

        if (submission.Assignment.Course.TeacherId != teacherId)
        {
            _logger.LogWarning("Security event: Teacher {TeacherId} attempted to grade a submission for assignment {AssignmentId} which belongs to a different teacher.", teacherId, submission.AssignmentId);
            return ServiceResult.Fail("غير مصرح لك بتقييم هذا التسليم.");
        }

        if (model.Grade < 0 || model.Grade > submission.Assignment.MaxGrade)
            return ServiceResult.Fail($"الدرجة يجب أن تكون بين 0 و {submission.Assignment.MaxGrade}.");

        submission.Grade = model.Grade;
        submission.TeacherComment = model.TeacherComment?.Trim();
        submission.Status = SubmitStatus.Graded;

        _submissionRepository.Update(submission);
        await _submissionRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }
}

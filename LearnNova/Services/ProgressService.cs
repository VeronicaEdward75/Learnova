using LearnNova.Models.Entities;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class ProgressService : IProgressService
{
    private readonly IContentProgressRepository _progressRepository;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICourseContentRepository _contentRepository;

    public ProgressService(
        IContentProgressRepository progressRepository,
        IEnrollmentService enrollmentService,
        ICourseContentRepository contentRepository)
    {
        _progressRepository = progressRepository;
        _enrollmentService = enrollmentService;
        _contentRepository = contentRepository;
    }

    public async Task<ServiceResult> MarkAsCompletedAsync(string studentId, int contentId)
    {
        var content = await _contentRepository.GetByIdAsync(contentId);
        if (content == null)
            return ServiceResult.Fail("المحتوى غير موجود.");

        // Rule: Only enrolled students can update progress.
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(studentId, content.CourseId);
        if (!isEnrolled)
            return ServiceResult.Fail("غير مصرح لك بتسجيل التقدم في هذا الكورس.");

        var existing = await _progressRepository.GetProgressAsync(studentId, contentId);
        if (existing != null)
        {
            if (!existing.IsCompleted)
            {
                existing.IsCompleted = true;
                _progressRepository.Update(existing);
                await _progressRepository.SaveChangesAsync();
            }
            return ServiceResult.Success();
        }

        var progress = new ContentProgress
        {
            StudentId = studentId,
            ContentId = contentId,
            IsCompleted = true,
            WatchedSeconds = null // basic completion tracking
        };

        await _progressRepository.AddAsync(progress);
        await _progressRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<List<int>> GetCompletedContentIdsAsync(string studentId, int courseId)
    {
        var progressList = await _progressRepository.GetStudentProgressInCourseAsync(studentId, courseId);
        return progressList.Where(p => p.IsCompleted).Select(p => p.ContentId).ToList();
    }

    public async Task<int> GetCourseProgressPercentageAsync(string studentId, int courseId, int totalContentsCount)
    {
        if (totalContentsCount == 0) return 0;
        var completedList = await GetCompletedContentIdsAsync(studentId, courseId);
        double percentage = (double)completedList.Count / totalContentsCount * 100;
        return (int)Math.Round(percentage);
    }

    public async Task<Dictionary<int, int>> GetCourseProgressPercentagesAsync(string studentId, IReadOnlyDictionary<int, int> contentCountsByCourse)
    {
        if (contentCountsByCourse.Count == 0)
            return new Dictionary<int, int>();

        var completedCounts = await _progressRepository.GetCompletedContentCountsByCoursesAsync(
            studentId,
            contentCountsByCourse.Keys);

        var result = new Dictionary<int, int>(contentCountsByCourse.Count);
        foreach (var (courseId, totalContentsCount) in contentCountsByCourse)
        {
            if (totalContentsCount == 0)
            {
                result[courseId] = 0;
                continue;
            }

            var completedCount = completedCounts.GetValueOrDefault(courseId, 0);
            result[courseId] = (int)Math.Round((double)completedCount / totalContentsCount * 100);
        }

        return result;
    }
}

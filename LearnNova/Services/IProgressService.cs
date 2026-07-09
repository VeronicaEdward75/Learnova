namespace LearnNova.Services;

public interface IProgressService
{
    Task<ServiceResult> MarkAsCompletedAsync(string studentId, int contentId);
    Task<List<int>> GetCompletedContentIdsAsync(string studentId, int courseId);
    Task<int> GetCourseProgressPercentageAsync(string studentId, int courseId, int totalContentsCount);
    Task<Dictionary<int, int>> GetCourseProgressPercentagesAsync(string studentId, IReadOnlyDictionary<int, int> contentCountsByCourse);
}

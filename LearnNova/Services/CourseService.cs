using LearnNova.Models.Entities;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;

    public CourseService(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public Task<IEnumerable<Course>> GetAllCoursesAsync() => _courseRepository.GetAllWithTeacherAsync();

    public Task<IEnumerable<Course>> SearchCoursesAsync(string? term, bool? isPublished) =>
        _courseRepository.SearchAsync(term, isPublished);

    public Task<Course?> GetCourseByIdAsync(int id) => _courseRepository.GetByIdAsync(id);

    public async Task<ServiceResult> SetPublishedStatusAsync(int id, bool isPublished)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
        {
            return ServiceResult.Fail("الكورس غير موجود");
        }

        course.IsPublished = isPublished;
        _courseRepository.Update(course);
        await _courseRepository.SaveChangesAsync();
        return ServiceResult.Success();
    }
}

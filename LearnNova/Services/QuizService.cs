using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepository;
    private readonly ICourseRepository _courseRepository;

    public QuizService(IQuizRepository quizRepository, ICourseRepository courseRepository)
    {
        _quizRepository = quizRepository;
        _courseRepository = courseRepository;
    }

    public async Task<IEnumerable<Quiz>> GetQuizzesForCourseAsync(int courseId, string teacherId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null || course.TeacherId != teacherId)
            return Enumerable.Empty<Quiz>();

        return await _quizRepository.GetByCourseIdAsync(courseId);
    }

    public async Task<Quiz?> GetQuizByIdAsync(int id, string teacherId)
    {
        var quiz = await _quizRepository.GetByIdWithCourseAsync(id);
        if (quiz == null || quiz.Course.TeacherId != teacherId)
            return null;

        return quiz;
    }

    public async Task<ServiceResult> CreateQuizAsync(QuizFormViewModel model, string teacherId)
    {
        var course = await _courseRepository.GetByIdAsync(model.CourseId);
        if (course == null)
            return ServiceResult.Fail("الكورس غير موجود.");

        if (course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بإضافة اختبار لهذا الكورس.");

        if (string.IsNullOrWhiteSpace(model.Title)) return ServiceResult.Fail("عنوان الاختبار مطلوب.");
        if (string.IsNullOrWhiteSpace(model.Description)) return ServiceResult.Fail("وصف الاختبار مطلوب.");
        if (model.TimeLimitMinutes <= 0) return ServiceResult.Fail("وقت الاختبار يجب أن يكون أكبر من صفر.");
        if (model.PassingScore < 0) return ServiceResult.Fail("درجة النجاح يجب أن تكون صفر أو أكثر.");
        if (model.MaxAttempts < 1) return ServiceResult.Fail("عدد المحاولات يجب أن يكون 1 على الأقل.");


        var quiz = new Quiz
        {
            Title = model.Title,
            Description = model.Description,
            TimeLimitMinutes = model.TimeLimitMinutes,
            PassingScore = model.PassingScore,
            MaxAttempts = model.MaxAttempts,
            CourseId = model.CourseId,
            IsPublished = model.IsPublished,
            ShowAnswers = model.ShowAnswers
        };

        await _quizRepository.AddAsync(quiz);
        await _quizRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateQuizAsync(QuizFormViewModel model, string teacherId)
    {
        var quiz = await _quizRepository.GetByIdWithCourseAsync(model.Id);
        if (quiz == null)
            return ServiceResult.Fail("الاختبار غير موجود.");

        if (quiz.Course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بتعديل هذا الاختبار.");

        if (string.IsNullOrWhiteSpace(model.Title)) return ServiceResult.Fail("عنوان الاختبار مطلوب.");
        if (string.IsNullOrWhiteSpace(model.Description)) return ServiceResult.Fail("وصف الاختبار مطلوب.");
        if (model.TimeLimitMinutes <= 0) return ServiceResult.Fail("وقت الاختبار يجب أن يكون أكبر من صفر.");
        if (model.PassingScore < 0) return ServiceResult.Fail("درجة النجاح يجب أن تكون صفر أو أكثر.");
        if (model.MaxAttempts < 1) return ServiceResult.Fail("عدد المحاولات يجب أن يكون 1 على الأقل.");


        quiz.Title = model.Title;
        quiz.Description = model.Description;
        quiz.TimeLimitMinutes = model.TimeLimitMinutes;
        quiz.PassingScore = model.PassingScore;
        quiz.MaxAttempts = model.MaxAttempts;
        quiz.IsPublished = model.IsPublished;
        quiz.ShowAnswers = model.ShowAnswers;

        _quizRepository.Update(quiz);
        await _quizRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteQuizAsync(int id, string teacherId)
    {
        var quiz = await _quizRepository.GetByIdWithCourseAsync(id);
        if (quiz == null)
            return ServiceResult.Fail("الاختبار غير موجود.");

        if (quiz.Course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بحذف هذا الاختبار.");

        _quizRepository.Remove(quiz);
        await _quizRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> TogglePublishStatusAsync(int id, string teacherId)
    {
        var quiz = await _quizRepository.GetByIdWithCourseAsync(id);
        if (quiz == null)
            return ServiceResult.Fail("الاختبار غير موجود.");

        if (quiz.Course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بتعديل حالة هذا الاختبار.");

        quiz.IsPublished = !quiz.IsPublished;

        _quizRepository.Update(quiz);
        await _quizRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }
}

using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class QuizQuestionService : IQuizQuestionService
{
    private readonly IQuizQuestionRepository _questionRepository;
    private readonly IQuizRepository _quizRepository;

    public QuizQuestionService(IQuizQuestionRepository questionRepository, IQuizRepository quizRepository)
    {
        _questionRepository = questionRepository;
        _quizRepository = quizRepository;
    }

    public async Task<IEnumerable<QuizQuestion>> GetQuestionsForQuizAsync(int quizId, string teacherId)
    {
        var quiz = await _quizRepository.GetByIdWithCourseAsync(quizId);
        if (quiz == null || quiz.Course.TeacherId != teacherId)
            return Enumerable.Empty<QuizQuestion>();

        return await _questionRepository.GetByQuizIdAsync(quizId);
    }

    public async Task<QuizQuestion?> GetQuestionByIdAsync(int id, string teacherId)
    {
        var question = await _questionRepository.GetByIdWithQuizAsync(id);
        if (question == null || question.Quiz.Course.TeacherId != teacherId)
            return null;

        return question;
    }

    public async Task<ServiceResult> CreateQuestionAsync(QuestionFormViewModel model, string teacherId)
    {
        var quiz = await _quizRepository.GetByIdWithCourseAsync(model.QuizId);
        if (quiz == null || quiz.Course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بإضافة سؤال لهذا الاختبار.");

        if (model.Points <= 0) return ServiceResult.Fail("النقاط يجب أن تكون أكبر من 0.");
        
        if (model.Type == QuestionType.TrueFalse)
        {
            if (model.CorrectAnswer != "صح" && model.CorrectAnswer != "خطأ")
                return ServiceResult.Fail("الإجابة الصحيحة لسؤال الصح والخطأ يجب أن تكون 'صح' أو 'خطأ'.");
        }
        else
        {
            if (model.CorrectAnswer == "A" || model.CorrectAnswer == "أ" || model.CorrectAnswer == "OptionA")
                model.CorrectAnswer = model.OptionA;
            else if (model.CorrectAnswer == "B" || model.CorrectAnswer == "ب" || model.CorrectAnswer == "OptionB")
                model.CorrectAnswer = model.OptionB;
            else if (model.CorrectAnswer == "C" || model.CorrectAnswer == "ج" || model.CorrectAnswer == "OptionC")
                model.CorrectAnswer = model.OptionC;
            else if (model.CorrectAnswer == "D" || model.CorrectAnswer == "د" || model.CorrectAnswer == "OptionD")
                model.CorrectAnswer = model.OptionD;

            var validOptions = new[] { model.OptionA, model.OptionB, model.OptionC, model.OptionD };
            if (!validOptions.Contains(model.CorrectAnswer))
                return ServiceResult.Fail("الإجابة الصحيحة يجب أن تتطابق مع أحد الخيارات المدخلة.");
        }

        var maxOrder = await _questionRepository.GetMaxOrderIndexAsync(model.QuizId);


        var question = new QuizQuestion
        {
            QuizId = model.QuizId,
            Text = model.Text,
            Type = model.Type,
            CorrectAnswer = model.CorrectAnswer,
            Points = model.Points,
            OrderIndex = maxOrder + 1
        };

        if (model.Type == QuestionType.TrueFalse)
        {
            question.OptionA = "صح";
            question.OptionB = "خطأ";
            question.OptionC = null;
            question.OptionD = null;
        }
        else
        {
            question.OptionA = model.OptionA;
            question.OptionB = model.OptionB;
            question.OptionC = model.OptionC;
            question.OptionD = model.OptionD;
        }

        await _questionRepository.AddAsync(question);
        await _questionRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateQuestionAsync(QuestionFormViewModel model, string teacherId)
    {
        var question = await _questionRepository.GetByIdWithQuizAsync(model.Id);
        if (question == null || question.Quiz.Course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بتعديل هذا السؤال.");

        if (model.Points <= 0) return ServiceResult.Fail("النقاط يجب أن تكون أكبر من 0.");
        
        if (model.Type == QuestionType.TrueFalse)
        {
            if (model.CorrectAnswer != "صح" && model.CorrectAnswer != "خطأ")
                return ServiceResult.Fail("الإجابة الصحيحة لسؤال الصح والخطأ يجب أن تكون 'صح' أو 'خطأ'.");
        }
        else
        {
            if (model.CorrectAnswer == "A" || model.CorrectAnswer == "أ" || model.CorrectAnswer == "OptionA")
                model.CorrectAnswer = model.OptionA;
            else if (model.CorrectAnswer == "B" || model.CorrectAnswer == "ب" || model.CorrectAnswer == "OptionB")
                model.CorrectAnswer = model.OptionB;
            else if (model.CorrectAnswer == "C" || model.CorrectAnswer == "ج" || model.CorrectAnswer == "OptionC")
                model.CorrectAnswer = model.OptionC;
            else if (model.CorrectAnswer == "D" || model.CorrectAnswer == "د" || model.CorrectAnswer == "OptionD")
                model.CorrectAnswer = model.OptionD;

            var validOptions = new[] { model.OptionA, model.OptionB, model.OptionC, model.OptionD };
            if (!validOptions.Contains(model.CorrectAnswer))
                return ServiceResult.Fail("الإجابة الصحيحة يجب أن تتطابق مع أحد الخيارات المدخلة.");
        }

        question.Text = model.Text;

        question.Type = model.Type;
        question.CorrectAnswer = model.CorrectAnswer;
        question.Points = model.Points;

        if (model.Type == QuestionType.TrueFalse)
        {
            question.OptionA = "صح";
            question.OptionB = "خطأ";
            question.OptionC = null;
            question.OptionD = null;
        }
        else
        {
            question.OptionA = model.OptionA;
            question.OptionB = model.OptionB;
            question.OptionC = model.OptionC;
            question.OptionD = model.OptionD;
        }

        _questionRepository.Update(question);
        await _questionRepository.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteQuestionAsync(int id, string teacherId)
    {
        var question = await _questionRepository.GetByIdWithQuizAsync(id);
        if (question == null || question.Quiz.Course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بحذف هذا السؤال.");

        var quizId = question.QuizId;
        _questionRepository.Remove(question);
        await _questionRepository.SaveChangesAsync();

        await ReorderQuestionsAsync(quizId);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> MoveUpAsync(int id, string teacherId)
    {
        var question = await _questionRepository.GetByIdWithQuizAsync(id);
        if (question == null || question.Quiz.Course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بتعديل هذا السؤال.");

        var allQuestions = (await _questionRepository.GetByQuizIdAsync(question.QuizId)).ToList();
        var index = allQuestions.FindIndex(q => q.Id == id);

        if (index > 0)
        {
            var previous = allQuestions[index - 1];
            
            var temp = question.OrderIndex;
            question.OrderIndex = previous.OrderIndex;
            previous.OrderIndex = temp;

            _questionRepository.Update(question);
            _questionRepository.Update(previous);
            await _questionRepository.SaveChangesAsync();
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> MoveDownAsync(int id, string teacherId)
    {
        var question = await _questionRepository.GetByIdWithQuizAsync(id);
        if (question == null || question.Quiz.Course.TeacherId != teacherId)
            return ServiceResult.Fail("غير مصرح لك بتعديل هذا السؤال.");

        var allQuestions = (await _questionRepository.GetByQuizIdAsync(question.QuizId)).ToList();
        var index = allQuestions.FindIndex(q => q.Id == id);

        if (index < allQuestions.Count - 1)
        {
            var next = allQuestions[index + 1];
            
            var temp = question.OrderIndex;
            question.OrderIndex = next.OrderIndex;
            next.OrderIndex = temp;

            _questionRepository.Update(question);
            _questionRepository.Update(next);
            await _questionRepository.SaveChangesAsync();
        }

        return ServiceResult.Success();
    }

    private async Task ReorderQuestionsAsync(int quizId)
    {
        var questions = (await _questionRepository.GetByQuizIdAsync(quizId)).OrderBy(q => q.OrderIndex).ToList();
        
        for (int i = 0; i < questions.Count; i++)
        {
            if (questions[i].OrderIndex != i + 1)
            {
                questions[i].OrderIndex = i + 1;
                _questionRepository.Update(questions[i]);
            }
        }
        await _questionRepository.SaveChangesAsync();
    }
}

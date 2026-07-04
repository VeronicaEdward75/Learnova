using LearnNova.Models.Entities;

namespace LearnNova.Models.ViewModels.Student;

public class QuizEngineViewModel
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; }
    
    public int AttemptId { get; set; }
    
    public List<QuizQuestionDto> Questions { get; set; } = new();
}

public class QuizQuestionDto
{
    public int Id { get; set; }
    public int OrderIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public Enums.QuestionType Type { get; set; }
    
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string? OptionC { get; set; }
    public string? OptionD { get; set; }
}

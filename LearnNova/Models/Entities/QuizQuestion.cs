using LearnNova.Models.Enums;

namespace LearnNova.Models.Entities;

public class QuizQuestion
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string OptionsJson { get; set; } = "[]";
    public string CorrectAnswer { get; set; } = string.Empty;
    public int Points { get; set; }

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
}

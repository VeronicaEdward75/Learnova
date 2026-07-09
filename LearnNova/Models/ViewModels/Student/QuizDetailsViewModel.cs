using LearnNova.Models.Entities;

namespace LearnNova.Models.ViewModels.Student;

public class QuizDetailsViewModel
{
    public Quiz Quiz { get; set; } = null!;
    public List<QuizAttempt> AttemptHistory { get; set; } = new();
    
    public int AttemptsUsed { get; set; }
    public int RemainingAttempts => System.Math.Max(0, Quiz.MaxAttempts - AttemptsUsed);
    public bool HasUncompletedAttempt { get; set; }
}

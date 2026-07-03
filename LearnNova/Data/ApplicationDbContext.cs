using LearnNova.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearnNova.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseContent> CourseContents => Set<CourseContent>();
    public DbSet<ContentProgress> ContentProgressRecords => Set<ContentProgress>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
    public DbSet<StudentQuestion> StudentQuestions => Set<StudentQuestion>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // All FKs pointing back at ApplicationUser are Restrict. ApplicationUser is referenced
        // both directly (StudentId/TeacherId/SenderId/ReceiverId) and indirectly through the
        // Course ownership chain, so leaving any of these as Cascade causes SQL Server to reject
        // the migration with "may cause cycles or multiple cascade paths". Deactivate users via
        // IsActive instead of deleting them.
        builder.Entity<Course>()
            .HasOne(c => c.Teacher)
            .WithMany(u => u.CoursesTaught)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ContentProgress>()
            .HasOne(p => p.Student)
            .WithMany(u => u.ContentProgressRecords)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContentProgress>()
            .HasOne(p => p.Content)
            .WithMany(c => c.ProgressRecords)
            .HasForeignKey(p => p.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuizAttempt>()
            .HasOne(a => a.Student)
            .WithMany(u => u.QuizAttempts)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuizAttempt>()
            .HasOne(a => a.Quiz)
            .WithMany(q => q.Attempts)
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AssignmentSubmission>()
            .HasOne(s => s.Student)
            .WithMany(u => u.AssignmentSubmissions)
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AssignmentSubmission>()
            .HasOne(s => s.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<StudentQuestion>()
            .HasOne(q => q.Student)
            .WithMany(u => u.Questions)
            .HasForeignKey(q => q.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentQuestion>()
            .HasOne(q => q.Course)
            .WithMany(c => c.Questions)
            .HasForeignKey(q => q.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ChatMessage>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChatMessage>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChatMessage>()
            .HasOne(m => m.Course)
            .WithMany(c => c.ChatMessages)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CourseContent>()
            .HasOne(cc => cc.Course)
            .WithMany(c => c.Contents)
            .HasForeignKey(cc => cc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Quiz>()
            .HasOne(q => q.Course)
            .WithMany(c => c.Quizzes)
            .HasForeignKey(q => q.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuizQuestion>()
            .HasOne(qq => qq.Quiz)
            .WithMany(q => q.Questions)
            .HasForeignKey(qq => qq.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Assignment>()
            .HasOne(a => a.Course)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Course>()
            .Property(c => c.Price)
            .HasPrecision(10, 2);
    }
}

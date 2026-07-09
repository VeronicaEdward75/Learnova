using System;
using System.Collections.Generic;
using LearnNova.Models.Entities;

namespace LearnNova.Models.ViewModels.Student
{
    public class TimelineEventViewModel
    {
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // e.g., "Course", "Assignment", "Quiz", "Certificate", "Wallet"
        public string IconClass { get; set; } = "bi-circle";
        public string ColorClass { get; set; } = "text-primary";
        public string Url { get; set; } = "#";
    }

    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;

        // 2. Statistics Cards
        public int EnrolledCoursesCount { get; set; }
        public int CompletedCoursesCount { get; set; }
        public int CertificatesCount { get; set; }
        public decimal WalletBalance { get; set; }
        public int PendingAssignmentsCount { get; set; }
        public int AvailableQuizzesCount { get; set; }

        // 3. Continue Learning
        public Course? LastActiveCourse { get; set; }
        public int LastActiveCourseProgress { get; set; }

        // 4. Upcoming Tasks
        public List<Assignment> UpcomingAssignments { get; set; } = new();
        public List<LearnNova.Models.ViewModels.Student.MyQuizCardViewModel> AvailableQuizzes { get; set; } = new();

        // 5. Recent Activity
        public List<TimelineEventViewModel> TimelineEvents { get; set; } = new();

        // 6. Recent Certificates
        public List<Certificate> RecentCertificates { get; set; } = new();

        // 7. Wallet Summary
        public List<LearnNova.Models.ViewModels.Teacher.WalletTransactionViewModel> RecentTransactions { get; set; } = new();

        // 8. Learning Progress
        public int OverallCompletionPercentage { get; set; }
        public int TotalLessonsCompleted { get; set; }
        public int TotalAssignmentsCompleted { get; set; }
        public int TotalQuizzesPassed { get; set; }
    }
}

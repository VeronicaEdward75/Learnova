namespace LearnNova.Models.Lookups;

// Single source of truth for the Egyptian-curriculum reference data the prototype hardcoded in
// script.js (STAGES/STAGE_GRADES/SUBJECTS). Used both to render Register's cascading selects and
// to derive ApplicationUser.GradeLabel server-side from Stage + GradeNum.
public static class CurriculumLookup
{
    public record GradeOption(int Num, string Label);

    public static readonly string[] Stages = ["ابتدائي", "اعدادي", "ثانوي"];

    public static readonly Dictionary<string, GradeOption[]> StageGrades = new()
    {
        ["ابتدائي"] =
        [
            new GradeOption(1, "الأول الابتدائي"),
            new GradeOption(2, "الثاني الابتدائي"),
            new GradeOption(3, "الثالث الابتدائي"),
            new GradeOption(4, "الرابع الابتدائي"),
            new GradeOption(5, "الخامس الابتدائي"),
            new GradeOption(6, "السادس الابتدائي"),
        ],
        ["اعدادي"] =
        [
            new GradeOption(1, "الأول الإعدادي"),
            new GradeOption(2, "الثاني الإعدادي"),
            new GradeOption(3, "الثالث الإعدادي"),
        ],
        ["ثانوي"] =
        [
            new GradeOption(1, "الأول الثانوي"),
            new GradeOption(2, "الثاني الثانوي"),
            new GradeOption(3, "الثالث الثانوي"),
        ],
    };

    public static readonly Dictionary<string, string> Subjects = new()
    {
        ["Math"] = "رياضيات",
        ["Physics"] = "فيزياء",
        ["Chemistry"] = "كيمياء",
        ["Arabic"] = "لغة عربية",
        ["English"] = "لغة إنجليزية",
        ["Science"] = "علوم",
        ["History"] = "تاريخ",
    };

    public static string? GetGradeLabel(string? stage, int? gradeNum)
    {
        if (stage is null || gradeNum is null || !StageGrades.TryGetValue(stage, out var grades))
        {
            return null;
        }

        return grades.FirstOrDefault(g => g.Num == gradeNum)?.Label;
    }
}

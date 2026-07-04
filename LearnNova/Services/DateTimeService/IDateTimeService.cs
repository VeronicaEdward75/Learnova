namespace LearnNova.Services.DateTimeService;

public interface IDateTimeService
{
    DateTime UtcNow();
    DateTime ToUtc(DateTime localDateTime);
    DateTime ToLocal(DateTime utcDateTime);
}

using System;

namespace LearnNova.Services.DateTimeService;

public class SystemDateTimeService : IDateTimeService
{
    public DateTime UtcNow()
    {
        return DateTime.UtcNow;
    }

    public DateTime ToUtc(DateTime localDateTime)
    {
        if (localDateTime.Kind == DateTimeKind.Utc)
        {
            return localDateTime;
        }
        
        // If it's already Local or Unspecified (coming from HTML input),
        // we convert it to UTC based on the Server's Local timezone as requested by the user.
        // Wait, if it's Unspecified, ToUniversalTime() assumes it is Local.
        return localDateTime.ToUniversalTime();
    }

    public DateTime ToLocal(DateTime utcDateTime)
    {
        if (utcDateTime.Kind == DateTimeKind.Local)
        {
            return utcDateTime;
        }

        // If it's Unspecified (read from DB), we explicitly treat it as UTC before conversion,
        // to avoid incorrect shifts if the server tries to assume it's Local.
        if (utcDateTime.Kind == DateTimeKind.Unspecified)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }

        return utcDateTime.ToLocalTime();
    }
}

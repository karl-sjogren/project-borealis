namespace Borealis.Web.HostedServices;

public abstract class DailyHostedService : TimedHostedService {
    protected DailyHostedService(TimeSpan timeOfDay, TimeProvider timeProvider, ILogger<DailyHostedService> logger)
        : base(TimeSpan.FromDays(1), CalculateWaitBeforeStart(timeProvider, timeOfDay), logger) {
    }

    internal static TimeSpan CalculateWaitBeforeStart(TimeProvider timeProvider, TimeSpan timeOfDay) {
        var now = timeProvider.GetUtcNow();
        if(timeOfDay >= now.TimeOfDay)
            return timeOfDay - now.TimeOfDay;

        return (timeOfDay - now.TimeOfDay).Add(TimeSpan.FromDays(1));
    }
}

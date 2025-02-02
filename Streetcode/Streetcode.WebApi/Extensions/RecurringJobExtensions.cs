using Hangfire;
using Streetcode.BLL.Interfaces.Audio;

namespace Streetcode.WebApi.Extensions;

public static class RecurringJobExtensions
{
    public static void AddCleanAudiosJob(this WebApplication app)
    {
        var recurringJobManager = app.Services.GetService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate(
            "Clean audio that are not used in streetcodes",
            () => app.Services.GetService<IAudioService>().CleanUnusedAudiosAsync(),
            app.Configuration.GetSection("RecurringJobs")["AudioCleaningFrequency"],
            TimeZoneInfo.Utc);
    }
}
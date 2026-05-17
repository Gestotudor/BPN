using System.Data.Common;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Startup;

public static class DatabaseStartupExtensions
{
    public static async Task RunStartupTaskWithRetryAsync(
        this IServiceProvider services,
        Func<IServiceProvider, CancellationToken, Task> startupTask,
        CancellationToken cancellationToken = default,
        int maxAttempts = 10,
        int delaySeconds = 3)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(startupTask);

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DatabaseStartup");

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                await startupTask(scope.ServiceProvider, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransientStartupException(ex))
            {
                logger.LogWarning(
                    ex,
                    "Database startup task failed on attempt {Attempt}/{MaxAttempts}. Retrying in {DelaySeconds} seconds.",
                    attempt,
                    maxAttempts,
                    delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
        }

        using var finalScope = services.CreateScope();
        await startupTask(finalScope.ServiceProvider, cancellationToken);
    }

    private static bool IsTransientStartupException(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbException or SocketException or TimeoutException or IOException)
            {
                return true;
            }
        }

        return false;
    }
}

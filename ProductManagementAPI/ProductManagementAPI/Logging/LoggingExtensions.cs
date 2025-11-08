using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ProductManagementAPI.Logging;

public static class LoggingExtensions
{
    public static void LogProductCreationMetrics(this ILogger logger, ProductCreationMetrics m)
    {
        var eventId = new EventId(LogEvents.ProductCreationCompleted, nameof(LogEvents.ProductCreationCompleted));
        var msg = string.Format(CultureInfo.InvariantCulture,
            "OperationId={0} Name={1} SKU={2} Category={3} ValidationMs={4:F0} DbSaveMs={5:F0} TotalMs={6:F0} Success={7} Error={8}",
            m.OperationId,
            m.ProductName,
            m.SKU,
            m.Category,
            m.ValidationDuration.TotalMilliseconds,
            m.DatabaseSaveDuration.TotalMilliseconds,
            m.TotalDuration.TotalMilliseconds,
            m.Success,
            m.ErrorReason ?? string.Empty
        );

        if (m.Success)
        {
            logger.LogInformation(eventId, msg);
        }
        else
        {
            logger.LogError(eventId, msg);
        }
    }
}
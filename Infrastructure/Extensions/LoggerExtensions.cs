using System.Text.Json;

namespace minesweeperAPI.Infrastructure.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogExtension<T>(this ILogger logger, T structValue,
           LogLevel logLevel = LogLevel.Information,
           bool writeIndented = true)
        {
            var options = new JsonSerializerOptions { WriteIndented = writeIndented };
            string json = JsonSerializer.Serialize(structValue, options);
            logger.Log(logLevel, 0, json, null, (s, e) => json);
        }

        public static void LogExtension<T>(this ILogger logger, string message, T structValue,
            LogLevel logLevel = LogLevel.Information,
            bool writeIndented = true)
        {
            var options = new JsonSerializerOptions { WriteIndented = writeIndented };
            string json = JsonSerializer.Serialize(structValue, options);
            logger.Log(logLevel, 0, json, null, (s, e) => message + Environment.NewLine + json);
        }
    }
}

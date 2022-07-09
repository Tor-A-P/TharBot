using Discord;

namespace TharBot.Handlers
{
    public static class LoggingHandler
    {
        public static async Task LogAsync(string src, LogSeverity severity, string? message, Exception? exception = null)
        {
            if (severity.Equals(null))
            {
                severity = LogSeverity.Warning;
            }
            await Append($"{GetSeverityString(severity)}", GetConsoleColor(severity));
            await Append($" [{SourceToString(src)}] ", ConsoleColor.DarkGray);

            if (exception == null)
            {
                if (!string.IsNullOrWhiteSpace(message))
                    await Append($"{message}\n", ConsoleColor.White);
                else await Append("Unknown error!", ConsoleColor.DarkRed);
            }
            else
                await Append($"{exception.Message ?? "Unknown"}\n{exception.StackTrace ?? "Unknown"}\n", GetConsoleColor(severity));
        }

        public static async Task LogCriticalAsync(string source, string? message, Exception? exc = null)
            => await LogAsync(source, LogSeverity.Critical, message, exc);

        public static async Task LogInformationAsync(string source, string? message)
            => await LogAsync(source, LogSeverity.Info, message);

        private static async Task Append(string message, ConsoleColor color)
        {
            await Task.Run(() => {
                Console.ForegroundColor = color;
                Console.Write(message);
            });
        }

        private static string SourceToString(string src)
        {
            return src.ToLower() switch
            {
                "discord" => "DISCD",
                "victoria" => "VICTR",
                "audio" => "AUDIO",
                "admin" => "ADMIN",
                "gateway" => "GTWAY",
                "blacklist" => "BLAKL",
                "lavanode_0_socket" => "LAVAS",
                "lavanode_0" => "LAVA#",
                "bot" => "BOTWN",
                "database" => "DATBS",
                _ => src,
            };
        }

        private static string GetSeverityString(LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Critical => "CRIT",
                LogSeverity.Debug => "DBUG",
                LogSeverity.Error => "EROR",
                LogSeverity.Info => "INFO",
                LogSeverity.Verbose => "VERB",
                LogSeverity.Warning => "WARN",
                _ => "UNKN",
            };
        }

        private static ConsoleColor GetConsoleColor(LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Critical => ConsoleColor.Red,
                LogSeverity.Debug => ConsoleColor.Magenta,
                LogSeverity.Error => ConsoleColor.DarkRed,
                LogSeverity.Info => ConsoleColor.Green,
                LogSeverity.Verbose => ConsoleColor.DarkCyan,
                LogSeverity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.White,
            };
        }
    }
}
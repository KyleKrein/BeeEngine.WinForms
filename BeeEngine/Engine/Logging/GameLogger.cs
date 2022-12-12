namespace BeeEngine;

public sealed class GameLogger: ILogger, IDisposable
{
    private const string DebugMsg = "[DEBUG] - ";

    private const string ErrorMsg = "[ERROR] - ";

    private const string InfoMsg = "[INFO] - ";

    private const string WarningMsg = "[WARNING] - ";

    private FileStream? _fileStream;

    public LogLevel CurrentLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// The console color to use when logging debug messages
    /// </summary>
    public ConsoleColor DebugLogColor { get; set; } = ConsoleColor.White;

    /// <summary>
    /// The console color to use when logging error messages
    /// </summary>
    public ConsoleColor ErrorLogColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// The file output path when logging towards a file, you can use {0} for string formatting, it'll automatically supply the date and time
    /// </summary>
    public string FileOutputPath { get; set; } = "./log {0}.txt";

    /// <summary>
    /// Includes the date in the log messages
    /// </summary>
    public bool IncludeDate { get; set; }

    /// <summary>
    /// Includes the time in the log messages
    /// </summary>
    public bool IncludeTime { get; set; } = true;

    /// <summary>
    /// The console color to use when logging information messages
    /// </summary>
    public ConsoleColor InformationLogColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>
    /// Log options enum, decides where logging should output
    /// </summary>
    public LogTo LogTo { get; set; } = LogTo.Console;

    /// <summary>
    /// The console color to use when logging warning messages
    /// </summary>
    public ConsoleColor WarningLogColor { get; set; } = ConsoleColor.Yellow;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Log(string o, LogLevel logLevel = LogLevel.Debug)
    {
        if (LogTo == LogTo.None || CurrentLogLevel == LogLevel.None) return;
        if (logLevel == LogLevel.Debug && (int)CurrentLogLevel < 4) return;
        if (logLevel == LogLevel.Information && (int)CurrentLogLevel < 3) return;
        if (logLevel == LogLevel.Warning && (int)CurrentLogLevel < 2) return;

        DateTime now = DateTime.Now;
        StringBuilder stringBuilder = new StringBuilder();
        if (IncludeDate || IncludeTime)
        {
            stringBuilder.Append($"[{GetDateTimeString(now)}] ");
        }
        GetPrefix(stringBuilder, logLevel);
        stringBuilder.Append(o);

        if (LogTo.HasFlag(LogTo.Console))
        {
            LogConsole(stringBuilder.ToString(), logLevel);
        }

        if (LogTo.HasFlag(LogTo.File))
        {
            LogFile(stringBuilder.ToString());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetDateTimeString(DateTime now)
    {
        string?[] strArr = { null, null };

        if (IncludeDate)
            strArr[0] = now.ToShortDateString();

        if (IncludeTime)
            strArr[1] = now.ToString("HH:mm:ss.fff");

        return string.Join(" ", strArr).Trim();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetPrefix(StringBuilder stringBuilder, LogLevel logLevel)
    {
        stringBuilder.Append(logLevel switch
        {
            LogLevel.Error => ErrorMsg,
            LogLevel.Information => InfoMsg,
            LogLevel.Warning => WarningMsg,
            LogLevel.Debug => DebugMsg,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogConsole(object o, LogLevel logLevel = LogLevel.Debug)
    {
        ConsoleColor oldColor = SwitchColor(logLevel);
        Console.WriteLine(o);
        Console.ForegroundColor = oldColor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogFile(object o)
    {
        _fileStream ??= new FileStream(string.Format(FileOutputPath, DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss")),
            FileMode.Create, FileAccess.Write);

        byte[] bytes = Encoding.Unicode.GetBytes(o + "\r\n");
        _fileStream.Write(bytes, 0, bytes.Length);
        _fileStream.Flush();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ConsoleColor SwitchColor(LogLevel logLevel)
    {
        ConsoleColor oldColor = Console.ForegroundColor;

        Console.ForegroundColor = logLevel switch
        {
            LogLevel.Debug => DebugLogColor,
            LogLevel.Information => InformationLogColor,
            LogLevel.Warning => WarningLogColor,
            LogLevel.Error => ErrorLogColor,
            _ => Console.ForegroundColor
        };

        return oldColor;
    }

    public void Dispose()
    {
        _fileStream?.Dispose();
    }
}
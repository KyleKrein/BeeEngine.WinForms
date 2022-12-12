namespace BeeEngine
{
    internal static class Log
    {
        private static readonly GameLogger _logger;

        static Log()
        {
            _logger = new GameLogger();
            #if DEBUG
            _logger.CurrentLogLevel = LogLevel.Debug;
            #endif
        }
        [Conditional("DEBUG")]
        public static void Info(string message)
        {
            _logger.Log(message, LogLevel.Information);
        }
        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            _logger.Log(message, LogLevel.Debug);
        }
        [Conditional("DEBUG")]
        public static void Warning(string message)
        {
            _logger.Log(message, LogLevel.Warning);
        }
        public static void Error(string message)
        {
#if DEBUG
            _logger.Log(message, LogLevel.Error);
#else
            throw new Exception(message);
#endif
            
        }
    }
}
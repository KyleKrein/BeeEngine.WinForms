namespace BeeEngine
{
    public sealed class GameLoggerBuilder
    {
        private GameLogger _logger;

        internal GameLoggerBuilder()
        {
            _logger = new GameLogger();
        }
    }
}
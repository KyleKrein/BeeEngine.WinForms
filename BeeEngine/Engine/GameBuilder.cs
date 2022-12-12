namespace BeeEngine
{
    public sealed class GameBuilder
    {
        private Game _game;

        internal GameBuilder()
        {
            _game = new();
        }

        public GameBuilder DoOnLoad(Action action)
        {
            _game.LoadAction = action;
            return this;
        }
        public GameBuilder DoOnUpdate(Action action)
        {
            _game.UpdateAction = action;
            return this;
        }
        public GameBuilder DoOnDraw(Action action)
        {
            _game.DrawAction = action;
            return this;
        }
        public GameBuilder DoOnFixedUpdate(Action action)
        {
            _game.FixedUpdateAction = action;
            return this;
        }

        public GameBuilder WithSettings(IGameSettings gameSettings)
        {
            _game.ApplyGameSettings(gameSettings);
            return this;
        }
        public GameBuilder WithSettings(Action<GameSettings> gameSettings)
        {
            GameSettings settings = new GameSettings();
            gameSettings.Invoke(settings);
            _game.ApplyGameSettings(settings);
            return this;
        }

        public GameBuilder WithTitle(string title)
        {
            _game.Title = title;
            return this;
        }

        public Game Build()
        {
            var temp = _game;
            _game = new Game();
            return temp;
        }
    }
}
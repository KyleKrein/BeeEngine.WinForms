using ValueObject;

namespace BeeEngine
{
    public sealed class GameWindowSize: ValueObject<int, GameWindowSize>
    {
        private bool _isHeight;
        private GameEngine _gameInstance;
        internal GameWindowSize(bool isHeight, GameEngine gameInstance)
        {
            _gameInstance = gameInstance;
            _isHeight = isHeight;
        }
        
        protected override bool Validate()
        {
            if (Value <= 0)
            {
                var message = _isHeight ? "Height" : "Width";
                throw new ArgumentException($"{message} of the Game Window can't be equal or less then zero");
            }

            _gameInstance.Window.Invoke((() =>
            {
                if (_isHeight)
                {
                    _gameInstance.Window.Height = Value;
                }
                else
                {
                    _gameInstance.Window.Width = Value;
                }
            }));
            
            _gameInstance._gameSettings.WeakChanged?.Invoke(_gameInstance._gameSettings, EventArgs.Empty);
            return true;
        }

        public override int Get()
        {
            return _isHeight ? _gameInstance.Window.Height : _gameInstance.Window.Width;
        }
    }
}
namespace BeeEngine
{
    public sealed class Game: GameEngine
    {
        internal Game()
        {
            
        }

        internal Action LoadAction { get; set; }
        internal Action UpdateAction { get; set; }
        internal Action DrawAction { get; set; }
        internal Action FixedUpdateAction { get; set; }
        private string _title;
        private bool _titleWasChanged = false;
        public string Title
        {
            get
            {
                if (Window != null)
                    return Window.Text;
                return _title;
            }
            set
            {
                if (Window != null)
                    Window.Text = value;
                _title = value;
            }
        }

        internal void ApplyGameSettings(IGameSettings gameSettings)
        {
            RegisterGameSettings(gameSettings);
        }
        protected override void OnLoad()
        {
            LoadAction?.Invoke();
        }

        protected override void OnUpdate()
        {
            UpdateAction?.Invoke();
        }

        protected override void OnDraw()
        {
            DrawAction?.Invoke();
        }

        protected override void OnFixedUpdate()
        {
            FixedUpdateAction?.Invoke();
        }

        protected override void GetKeyDown(KeyEventArgs e)
        {
            
        }

        protected override void GetKeyUp(KeyEventArgs e)
        {
            
        }

        protected override void GetMouseClick(MouseEventArgs e)
        {
            
        }

        protected override void GetMouseWheel(MouseEventArgs e)
        {
            
        }
    }
}
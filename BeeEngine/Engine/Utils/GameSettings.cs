namespace BeeEngine;

public class GameSettings : IGameSettings
{
    public GameWindowSize WindowWidth { get; set; }
    public GameWindowSize WindowHeight { get; set; }

    public GameSettings()
    {
        _weakChanged = new WeakEvent<EventArgs>();
    }
    private byte _fps;
    public byte Fps 
    {
        get => _fps;
        set
        {
            _fps = value;
            SomethingWasChanged();
        } 
    }

    private Color _color;
    public Color BackgroundColor
    {
        get => _color;
        set
        {
            _color = value;
            SomethingWasChanged();
        } 
    }
    public event EventHandler<EventArgs>? Changed;
    private WeakEvent<EventArgs> _weakChanged;
    public WeakEvent<EventArgs> WeakChanged
    {
        get => _weakChanged;
        set
        {
            if (value.EventID == _weakChanged.EventID)
            {
                _weakChanged = value;
                return;
            }

            throw new ArgumentException("You can't override the basic WeakEvent");
        } 
    }

    private void SomethingWasChanged()
    {
        WeakChanged?.Invoke(this, EventArgs.Empty);
    }
}
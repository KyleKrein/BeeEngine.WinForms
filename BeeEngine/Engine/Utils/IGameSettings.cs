namespace BeeEngine
{
    public interface IGameSettings
    {
        GameWindowSize WindowWidth { get; set; }
        GameWindowSize WindowHeight { get; set; }
        byte Fps { get; set; }
        Color BackgroundColor { get; set; }
        WeakEvent<EventArgs> WeakChanged { get; set; }
    }
}
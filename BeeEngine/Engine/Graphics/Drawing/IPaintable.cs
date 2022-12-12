namespace BeeEngine.Drawing
{
    public interface IPaintable
    {
        void Show();
        void Hide();
        /// <summary>
        /// Makes the drawn region invalid and repaints itself asyncronously
        /// </summary>
        void Invalidate();
        /// <summary>
        /// Forces a Syncronous repaint
        /// </summary>
        void Refresh();
    }
}

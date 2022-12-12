namespace BeeEngine.Drawing
{
    public abstract class BaseDrawnableObject
    {
        public Transform Transform { get; set; }

        public BaseDrawnableObject()
        {
            Transform = new Transform(this);
        }
        public bool IsDrawn { get; private set; }
        public virtual void Show()
        {
            IsDrawn = true;
        }
        public virtual void Hide()
        {
            IsDrawn = false;
        }
        protected abstract void OnPaint(FastGraphics g);
        internal void PaintIt(FastGraphics g)
        {
            if (!IsDrawn)
                return;
            OnPaint(g);
        }
    }
}

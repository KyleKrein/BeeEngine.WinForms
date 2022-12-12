namespace BeeEngine.Drawing
{
    public abstract class DrawnableObject : BaseDrawnableObject, IPaintable
    {
        List<DrawnableObject> children;
        private bool isInvalid = true;
        protected readonly object locker = new object();
        public bool IsInvalid { 
            get
            {
                lock(locker)
                {
                    return isInvalid;
                }
            }
            protected set
            {
                lock(locker)
                {
                    isInvalid = value;
                }
            }
        }
        public WeakEvent<FastGraphics> WeakPaint { get; set; }
        public WeakEvent<MouseEventArgs> WeakMouseMove { get; set; }
        public WeakEvent<MouseEventArgs> WeakMouseClick { get; set; }
        public WeakEvent<MouseEventArgs> WeakMouseDown { get; set; }
        public WeakEvent<MouseEventArgs> WeakMouseUp { get; set; }
        public WeakEvent<MouseEventArgs> WeakMouseEnter { get; set; }
        public WeakEvent<MouseEventArgs> WeakMouseLeave { get; set; }

        public DrawnableObject()
        {
            WeakPaint = new WeakEvent<FastGraphics>();
            WeakMouseMove = new WeakEvent<MouseEventArgs>();
            WeakMouseClick = new WeakEvent<MouseEventArgs>();
            WeakMouseDown = new WeakEvent<MouseEventArgs>();
            WeakMouseUp = new WeakEvent<MouseEventArgs>();
            WeakMouseEnter = new WeakEvent<MouseEventArgs>();
        }

        public virtual void Invalidate()
        {
            IsInvalid = true;
        }

        public virtual void Refresh()
        {
            
        }
    }
}

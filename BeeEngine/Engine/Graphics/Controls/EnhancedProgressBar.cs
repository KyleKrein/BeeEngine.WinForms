using BeeEngine.Vector;

namespace BeeEngine.Controls
{
    public enum TextPosition : byte
    {
        Left,
        Right,
        Center,
        Sliding,
        None
    }

    public sealed class EnhancedProgressBar : ProgressBar
    {
        //Поля
        private Color channelColor = Color.LightSteelBlue;

        private Color sliderColor = Color.RoyalBlue;
        private Color foreBackColor = Color.RoyalBlue;
        private int channelHeight = 6;
        private int sliderHeight = 6;
        private TextPosition showValue = TextPosition.Right;
        public Vector2 pos;
        public Vector2 scale;

        public Color ChannelColor
        {
            get => channelColor;
            set
            {
                channelColor = value;
                this.Invalidate();
            }
        }

        public Color SliderColor
        {
            get => sliderColor;
            set
            {
                sliderColor = value;
                this.Invalidate();
            }
        }

        public Color ForeBackColor
        {
            get => foreBackColor;
            set
            {
                foreBackColor = value;
                this.Invalidate();
            }
        }

        public int ChannelHeight
        {
            get => channelHeight;
            set
            {
                channelHeight = value;
                this.Invalidate();
            }
        }

        public int SliderHeight
        {
            get => sliderHeight;
            set
            {
                sliderHeight = value;
                this.Invalidate();
            }
        }

        public TextPosition ShowValue
        {
            get => showValue;
            set
            {
                showValue = value;
                this.Invalidate();
            }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override Font Font
        {
            get => base.Font;
            set => base.Font = value;
        }

        private bool paintedBack;
        public bool stopPainting;

        //Конструктор
        public EnhancedProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);//это нужно чтобы обновление прогресс бара контроллировалось программой, а не операционной системой
            this.ForeColor = Color.White;
        }

        //отрисовка фона и канала
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (stopPainting == false)
            {
                if (paintedBack == false)
                {
                    Graphics g = pevent.Graphics;
                    Rectangle rectChannel = new Rectangle(0, 0, this.Width, channelHeight);

                    SolidBrush brushChannel = new SolidBrush(channelColor);
                    if (channelHeight >= sliderHeight) { rectChannel.Y = this.Height = channelHeight; }
                    else { rectChannel.Y = this.Height - ((channelHeight - sliderHeight) / 2); }
                    //отрисовка
                    g.Clear(this.Parent.BackColor);
                    g.FillRectangle(brushChannel, rectChannel);

                    paintedBack = true;
                }
                if (this.Value == this.Maximum || this.Value == this.Minimum) { paintedBack = false; }
            }
        }

        //отрисовка слайдера
        protected override void OnPaint(PaintEventArgs e)
        {
            if (stopPainting == false)
            {
                Graphics g = e.Graphics;
                double scale = (((double)this.Value - this.Minimum) / ((double)this.Maximum - this.Minimum));
                int sliderWidth = (int)(this.Width * scale);
                Rectangle rectSlider = new Rectangle(0, 0, sliderWidth, sliderHeight);
                SolidBrush brushSlider = new SolidBrush(sliderColor);
                if (sliderHeight >= channelHeight) { rectSlider.Y = this.Height - sliderHeight; }
                else { rectSlider.Y = this.Height - (sliderHeight + channelHeight) / 2; }

                //Отрисовка
                if (sliderWidth > 1) { g.FillRectangle(brushSlider, rectSlider); }
                if (showValue != TextPosition.None) { DrawValueText(g, sliderWidth, rectSlider); }
            }
            //остановка отрисовки, если прогресс достигает максимума
            if (this.Value == this.Maximum) { stopPainting = true; }
            else { stopPainting = false; }
        }

        //отрисовка текста
        private void DrawValueText(Graphics g, int sliderWidth, Rectangle rectSlider)
        {
            string text = $"{this.Value}/{this.Maximum}";
            var textSize = TextRenderer.MeasureText(text, this.Font);
            var rectText = new Rectangle(0, 0, textSize.Width, textSize.Height + 2);
            SolidBrush brushText = new SolidBrush(this.ForeColor);
            SolidBrush brushTextBack = new SolidBrush(foreBackColor);
            var textFornat = new StringFormat();
            switch (showValue)
            {
                case TextPosition.Left:
                    rectText.X = 0;
                    textFornat.Alignment = StringAlignment.Near;
                    break;

                case TextPosition.Right:
                    rectText.X = this.Width - textSize.Width;
                    textFornat.Alignment = StringAlignment.Far;
                    break;

                case TextPosition.Center:
                    rectText.X = (this.Width - textSize.Width) / 2;
                    textFornat.Alignment = StringAlignment.Center;
                    break;

                case TextPosition.Sliding:
                    rectText.X = sliderWidth - textSize.Width;
                    textFornat.Alignment = StringAlignment.Center;

                    SolidBrush brushClear = new SolidBrush(this.Parent.BackColor);
                    var rect = rectSlider;
                    rect.Y = rectText.Y;
                    rect.Height = rectText.Height;
                    g.FillRectangle(brushClear, rect);
                    break;
            }
            //Отрисовка
            g.FillRectangle(brushTextBack, rectText);
            g.DrawString(text, this.Font, brushText, rectText, textFornat);
        }

        public void SetPosition(int x, int y, int width, int height)
        {
            this.pos = new Vector2(x, y);
            this.scale = new Vector2(width, height);
            this.SetBounds(x, y, width, height);
        }
    }
}
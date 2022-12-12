using BeeEngine.Vector;

namespace BeeEngine.Controls
{
    public sealed class EnhancedButton : Button
    {
        //Поля
        private int borderSize;

        private int borderRadius;
        private Color borderColor = Color.Black;

        //Это необходимо для правильного отображения кнопки в случае изменения ее параметров
        public int BorderSize
        {
            get => borderSize;
            set
            {
                borderSize = value;
                this.Invalidate();
            }
        }

        public int BorderRadius
        {
            get => borderRadius;
            set
            {
                if (value <= this.Height) { borderRadius = value; }
                else { borderRadius = this.Height; }
                this.Invalidate();
            }
        }

        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                this.Invalidate();
            }
        }

        private Vector2 paintCoords = Vector2.Zero();
#if MAC
        private Label TextLabel;
        //private static SimpleNeuron neuron = new SimpleNeuron();
        //static EnhancedButton()
        //{
        //    //neuron.Init(0.0043750414890097434863000107m);
        //}
        //private float GetXLocationModifier(int sizeX)
        //{
        //    var result = neuron.ProcessInputData(sizeX);
        //    result = Math.Round(result, 3);
        //    return (float)result;
        //}
        private bool _useSystemAppearance = true;
        public float locationXModifier = 1;
        public bool useSystemAppearance
        {
            get
            {
                return _useSystemAppearance;
            }
            set
            {
                if(value == true && TextLabel!=null)
                {
                    base.Text = TextLabel.Text;
                    base.ForeColor = TextLabel.ForeColor;
                    Controls.Remove(TextLabel);
                    TextLabel = null;
                }
                if(value == false)
                {
                    TextLabel = new Label();
                    TextLabel.BackColor = BackColor;
                    tempBackColor = BackColor;
                    TextLabel.ForeColor = base.ForeColor;
                    base.ForeColor = Color.Empty;
                    TextLabel.Text = base.Text;
                    base.Text = "";
                    TextLabel.TextAlign = base.TextAlign;
                    //TextLabel.Dock = DockStyle.Fill;
                    TextLabel.Location = new Point(0, 0);
                    TextLabel.Size = this.Size;
                    this.SizeChanged += EnhancedButton_SizeChanged;
                    TextLabel.Paint += PaintOnLabel;
                    //TextLabel.MouseHover += TextLabel_MouseHover;
                    TextLabel.MouseEnter += TextLabel_MouseHover;
                    TextLabel.MouseLeave += TextLabel_MouseLeave;
                    Controls.Add(TextLabel);
                }
                _useSystemAppearance = value;
                //Invalidate();
            }
        }
        private Color tempBackColor;
        private void TextLabel_MouseLeave(object sender, EventArgs e)
        {
            TextLabel.BackColor = tempBackColor;
        }

        private void TextLabel_MouseHover(object sender, EventArgs e)
        {
            /*ColorMatrix normalColor = float[,]{
                new float[1,0,0,]
            }*/
            TextLabel.BackColor = SystemColors.ButtonHighlight;
        }

        private void PaintOnLabel(object sender, PaintEventArgs p)
        {
                p.Graphics.SmoothingMode = SmoothingMode.None;

                RectangleF rectSurface = new RectangleF(0, 0, TextLabel.Width, TextLabel.Height);
                RectangleF rectBorder = new RectangleF(1, 1, TextLabel.Width - 0.8f, TextLabel.Height - 1);

                if (borderRadius > 2)//Круглая кнопка (Ну или хотя бы со скругленными краями)
                {
                    GraphicsPath pathSurface = getFigureGraphicsPath(rectSurface, borderRadius);
                    GraphicsPath pathBorder = getFigureGraphicsPath(rectBorder, borderRadius - 1f);
                    Pen penSurface = new Pen(TextLabel.BackColor, 2);
                    Pen penBorder = new Pen(borderColor, borderSize);
                    penBorder.Alignment = PenAlignment.Inset;
                    //основная часть кнопки
                    this.Region = new Region(pathSurface);
                    //Отрисовка в высоком разрешении (по факту все все равно выглядит отстойно)
                    p.Graphics.DrawPath(penSurface, pathSurface);

                    //граница кнопки
                    if (borderSize >= 1) { p.Graphics.DrawPath(penBorder, pathBorder); }
                }
                else//обычная кнопка
                {
                    //основная часть кнопки
                    this.Region = new Region(rectSurface);
                    //граница кнопки
                    if (borderSize >= 1)
                    {
                        Pen penBorder = new Pen(borderColor, borderSize);
                        penBorder.Alignment = PenAlignment.Inset;
                        p.Graphics.DrawRectangle(penBorder, 0, 0, this.Width - 1, this.Height - 1);
                    }
            }
        }
        private void EnhancedButton_SizeChanged(object sender, EventArgs e)
        {
            if(!useSystemAppearance)
            {
                //GetXLocationModifier(Size.Width)
                TextLabel.Size = new Size((int)(this.Size.Width*locationXModifier), (int)(this.Size.Height * 1));
                //paintCoords = new Vector2((int)(this.Size.Width * 0.03f), this.Size.Height*0);
                //TextLabel.Location = new Point((int)(Width*.05f), 0);
            }
        }

        public new Color ForeColor
        {
            get
            {
                if(useSystemAppearance)
                {
                    return base.ForeColor;
                }
                return TextLabel.ForeColor;
            }
            set
            {
                if(useSystemAppearance)
                {
                    base.ForeColor = value;
                }
                else
                {
                    TextLabel.ForeColor = value;
                }
            }
        }
        public override Color BackColor
        {
            get
            {
                if (useSystemAppearance)
                {
                    return base.BackColor;
                }
                return TextLabel.BackColor;
            }
            set
            {
                if (useSystemAppearance)
                {
                    base.BackColor = value;
                }
                else
                {
                    TextLabel.BackColor = value;
                    tempBackColor = value;
                }
            }
        }
        public new string Text
        {
            get
            {
                if (useSystemAppearance)
                {
                    return base.Text;
                }
                else
                {
                    return TextLabel.Text;
                }
            }
            set
            {
                if (useSystemAppearance)
                {
                    base.Text = value;
                }
                else
                {
                    TextLabel.Text = value;
                    base.Text = "";
                }
            }
        }
        public new event EventHandler Click
        {
            add
            {
                base.Click += value;
                if(!useSystemAppearance)
                {
                    TextLabel.Click += value;
                }
            }
            remove
            {
                base.Click += value;
                if (!useSystemAppearance)
                {
                    TextLabel.Click -= value;
                }
            }
        }

#endif

        //Конструктор
        public EnhancedButton()
        {
#if MAC
            this.useSystemAppearance = true;
#endif
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;

            this.Size = new Size(150, 40);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
            this.Resize += Button_Resize;
        }

        //методы
        private GraphicsPath getFigureGraphicsPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            //Тут душные мат формулы для отрисовки разных частей окружности
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Width - radius, rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

#if !MAC

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            PaintButton(pevent);
        }

#endif

        private void PaintButton(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.None;

            RectangleF rectSurface = new RectangleF(paintCoords.X, paintCoords.Y, this.Width, this.Height);
            RectangleF rectBorder = new RectangleF(paintCoords.X + 1, paintCoords.Y + 1, this.Width - 0.8f, this.Height - 1);

            if (borderRadius > 2)//Круглая кнопка (Ну или хотя бы со скругленными краями)
            {
                GraphicsPath pathSurface = getFigureGraphicsPath(rectSurface, borderRadius);
                GraphicsPath pathBorder = getFigureGraphicsPath(rectBorder, borderRadius - 1f);
                Pen penSurface = new Pen(this.Parent.BackColor, 2);
                Pen penBorder = new Pen(borderColor, borderSize);
                penBorder.Alignment = PenAlignment.Inset;
                //основная часть кнопки
                this.Region = new Region(pathSurface);
                //Отрисовка в высоком разрешении (по факту все все равно выглядит отстойно)
                pevent.Graphics.DrawPath(penSurface, pathSurface);

                //граница кнопки
                if (borderSize >= 1) { pevent.Graphics.DrawPath(penBorder, pathBorder); }
            }
            else//обычная кнопка
            {
                //основная часть кнопки
                this.Region = new Region(rectSurface);
                //граница кнопки
                if (borderSize >= 1)
                {
                    Pen penBorder = new Pen(borderColor, borderSize);
                    penBorder.Alignment = PenAlignment.Inset;
                    pevent.Graphics.DrawRectangle(penBorder, 0, 0, this.Width - 1, this.Height - 1);
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.Parent.BackColorChanged += Container_BackColorChanged;
        }

        private void Button_Resize(object sender, EventArgs e)
        {
            if (this.BorderRadius > this.Height) { this.BorderRadius = this.Height; }
        }

        private void Container_BackColorChanged(object sender, EventArgs e) => this.Invalidate();
    }
}
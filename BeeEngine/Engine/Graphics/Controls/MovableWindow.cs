using BeeEngine.Vector;

namespace BeeEngine.Controls
{
    public class MovableWindow : UserControl
    {
        public EnhancedButton CloseButton;
        public Label Title;
        public Panel UpPanel;
        public Panel MainPanel;
        public Panel BottomPanel;

        public MovableWindow()
        {
            //this.DoubleBuffered = true;
            //ImageHelper.SetDoubleBuffered(this);
            //Enable these styles to reduce flicker
            //1. Enable user paint.
            /*this.SetStyle(ControlStyles.UserPaint, true);
            //2. Enable double buffer.
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //3. Ignore a windows erase message to reduce flicker.
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);*/
            this.ForeColor = Color.Black;
            SuspendLayout();
            InitCanvas(Vector2.Zero(), new Vector2(1, 1));
            ResumeLayout();
        }

        private void InitCanvas(Vector2 Pos, Vector2 Scale, string Title = "Title")
        {
            //MovableWindow
            this.SetBounds((int)Pos.X, (int)Pos.Y, (int)Scale.X, (int)Scale.Y);
            this.Left = (GameApplication.Instance.Window.Width - this.Width) / 2;
            this.Top = (GameApplication.Instance.Window.Height - this.Height) / 2;
            this.SizeChanged += OnSizeChanged_Canvas;
            GameApplication.Instance.Window.Controls.Add(this);

            //MainPanel
            MainPanel = new Panel();
            MainPanel.Size = new Size(0, 0);
            MainPanel.Dock = DockStyle.Fill;
            MainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MainPanel.AutoSize = true;
            MainPanel.BackColor = Color.Transparent;
            MainPanel.AutoScroll = true;
            this.Controls.Add(MainPanel);
            //ImageHelper.SetDoubleBuffered(MainPanel);

            //UpperPanel
            UpPanel = new Panel();
            UpPanel.Dock = DockStyle.Top;
            UpPanel.Size = new Size(0, 0);
            UpPanel.AutoSize = true;
            UpPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            UpPanel.BackColor = Color.Transparent;
            //ImageHelper.SetDoubleBuffered(UpPanel);

            this.Controls.Add(UpPanel);

            //CloseButton
            Bitmap CloseButtonImage;
            if (File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Assets/closebutton.png")))
            {
                CloseButtonImage = (Bitmap)Image.FromFile(Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Assets/closebutton.png"));
            }
            else
            {
                CloseButtonImage = new Bitmap(50, 50);
                using (var g = Graphics.FromImage(CloseButtonImage))
                {
                    g.Clear(Color.Red);
                    g.Flush();
                    g.Dispose();
                }
            }
            CloseButton = new EnhancedButton();
            CloseButton.Image = CloseButtonImage;
            CloseButton.SetBounds((int)Scale.X - 50, 0, 50, 50);
#if MAC
            CloseButton.Click += CloseButton_MouseClick;
#endif
#if !MAC
            CloseButton.MouseClick += CloseButton_MouseClick;
#endif

            CloseButton.BackColor = Color.Transparent;
            CloseButton.TabStop = false;
            UpPanel.Controls.Add(CloseButton);

            //TitleLabel
            this.Title = new Label();
            this.Title.Text = Title;
            this.Title.Dock = DockStyle.Top;
            this.Title.SetBounds(0, 0, this.Width, 50);
            this.Title.BackColor = Color.Transparent;
            this.Title.TextAlign = ContentAlignment.MiddleCenter;
            this.Title.TextChanged += Title_TextChanged;
            UpPanel.Controls.Add(this.Title);
            
            this.Title.MoveAnotherOnMouseMove(this);

            //BottomPanel
            BottomPanel = new Panel();
            BottomPanel.Dock = DockStyle.Bottom;
            BottomPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BottomPanel.AutoSize = true;
            BottomPanel.BackColor = Color.Transparent;
            this.Controls.Add(BottomPanel);
        }

        private void Title_TextChanged(object sender, EventArgs e)
        {
            Size s = TextRenderer.MeasureText(Title.Text, Title.Font);
            this.Title.Size = new Size(Title.Width, s.Height);
        }

        private void OnSizeChanged_Canvas(object sender, EventArgs e)
        {
            this.Title.SetBounds(0, this.CloseButton.Height, this.Width, 40);
            this.CloseButton.SetBounds(this.Width - 50, 0, 50, 50);
        }

        private void CloseButton_MouseClick(object sender, MouseEventArgs e)
        {
            CloseWindow();
        }

        private void CloseButton_MouseClick(object sender, EventArgs e)
        {
            CloseWindow();
        }

        public virtual void CloseWindow()
        {
            GameApplication.Instance.Window.Controls.Remove(this);
            this.Dispose();
        }
    }
}
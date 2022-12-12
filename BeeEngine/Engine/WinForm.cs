using BeeEngine.Drawing;
using Timer = System.Timers.Timer;

namespace BeeEngine;

public sealed class WinForm : Form
{
    public BackgroundWorker MessageReceiver;

    public Label FPSCounterLabel;

#if !MAC
    private readonly HandleRef hDCRef;
    private readonly Graphics hDCGraphics;
    private readonly RazorPainter RP;
#endif
    internal FastGraphics FrameFastGFX;

#if !MAC

    /// <summary>
    /// root Bitmap
    /// </summary>
    public Bitmap FrameBMP { get; private set; }

    /// <summary>
    /// Graphics object to paint on RazorBMP
    /// </summary>
    public Graphics FrameGFX { get; private set; }

    /// <summary>
    /// Lock it to avoid resize/repaint race
    /// </summary>
    public readonly object RazorLock = new object();

#endif
    private readonly RenderingQueue _renderingQueue;
    private WinForm(){}
    //Двойная буфферизация нужна для снижения мерцания при обновлении окна приложения
    internal WinForm(RenderingQueue renderingQueue)
    {
        _renderingQueue = renderingQueue;
        //Enable these styles to reduce flicker
        //1. Enable user paint.
        /*this.SetStyle(ControlStyles.UserPaint, true);
        //2. Enable double buffer.
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        //3. Ignore a windows erase message to reduce flicker.
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);*/

        SetStyle(ControlStyles.DoubleBuffer, false);
        SetStyle(ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.Opaque, true);
#if !MAC
        hDCGraphics = CreateGraphics();
        hDCRef = new HandleRef(hDCGraphics, hDCGraphics.GetHdc());

        RP = new RazorPainter();
        FrameBMP = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppArgb);
        FrameGFX = Graphics.FromImage(FrameBMP);
        FrameFastGFX = FastGraphics.FromImage(FrameBMP, FrameGFX);

        this.Resize += (sender, args) =>
        {
            lock (RazorLock)
            {
                if (FrameGFX != null) FrameGFX.Dispose();
                if (FrameBMP != null) FrameBMP.Dispose();
                if (FrameFastGFX != null) FrameFastGFX.Dispose();
                FrameBMP = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppArgb);
                FrameGFX = Graphics.FromImage(FrameBMP);
                FrameFastGFX = FastGraphics.FromImage(FrameBMP, FrameGFX);
            }
        };

#else
        //currentContext = BufferedGraphicsManager.Current;
#endif

        //this.AutoScaleMode = AutoScaleMode.Dpi;
        //this.AutoScaleDimensions = new SizeF(1920, 1080);
        //SuspendLayout();
#if MAC
        ForeColor = Color.Black;
        BackColor = Color.Black;
#endif

        FPSCounterLabel = new Label();

        Controls.Add(FPSCounterLabel);
        FPSCounterLabel.BackColor = Color.Transparent;
        FPSCounterLabel.ForeColor = Color.Red;
        FPSCounterLabel.Font = new Font("TimesNewRoman", 16);
        FPSCounterLabel.AutoSize = true;
        //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        //this.WindowState = FormWindowState.Normal;
        //SetStyle(ControlStyles.CacheText, true);
        //this.TabIndex = 0;
        //UseNativeControl = true;
        fpstimer = new Timer(1000);
        fpstimer.Elapsed += (sender1, args) =>
        {
            if (Controls.Contains(FPSCounterLabel))
            {
                Update(delegate
                {
                    FPSCounterLabel.Text = "FPS: " + Fps; Fps = 0;
                });
            }
        };
        fpstimer.Start();
    }

    private Timer fpstimer;
    internal int Fps { get; private set; }
#if MAC
    //internal BufferedGraphicsContext currentContext;
    //internal BufferedGraphics myBuffer;
#endif

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x02000000;    // Turn on WS_EX_COMPOSITED
            return cp;
        }
    }

    protected override bool ShowWithoutActivation
    {
        get { return true; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnterFullScreen()
    {
        FullScreen.Enter(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LeaveFullScreen()
    {
        FullScreen.Leave(this);
    }

#if !MAC

    private void Paint()
    {
        RP.Paint(hDCRef, FrameBMP);
    }

#endif

    protected override void OnPaint(PaintEventArgs e)
    {
        //SuspendLayout();
#if MAC
        //if (myBuffer != null)
        //{
        //    FrameFastGFX ??= FastGraphics.FromGraphics(e.Graphics);
        //    GameEngine.BufferedRenderer(FrameFastGFX);
        //    myBuffer.Graphics.ResetTransform();
        //    myBuffer.Render(e.Graphics);

        //}
        FrameFastGFX?.Dispose();
        FrameFastGFX = FastGraphics.FromGraphics(e.Graphics);
        Camera.GoToWORK();
        RenderingQueue.Render(FrameFastGFX);
        base.OnPaint(e);
#else

        Camera.GoToWORK();
        _renderingQueue.Render(FrameFastGFX);
        Paint();
        base.OnPaint(new PaintEventArgs(FrameGFX, ClientRectangle));
#endif
        //ResumeLayout();
        Time.UpdateDeltaTime();
        Fps++;
    }

    private void Update(MethodInvoker callback)
    {
        if (IsDisposed || Disposing)
            return;

        try
        {
            if (this.InvokeRequired)
                this.Invoke(callback);
            else
                callback();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }
}

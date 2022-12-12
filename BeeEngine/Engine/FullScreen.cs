namespace BeeEngine;

internal static class FullScreen
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Enter(Form targetForm)
    {
        targetForm.WindowState = FormWindowState.Normal;
        targetForm.FormBorderStyle = FormBorderStyle.None;
        targetForm.WindowState = FormWindowState.Maximized;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Leave(Form targetForm)
    {
        targetForm.FormBorderStyle = FormBorderStyle.Sizable;
        targetForm.WindowState = FormWindowState.Normal;
    }
}

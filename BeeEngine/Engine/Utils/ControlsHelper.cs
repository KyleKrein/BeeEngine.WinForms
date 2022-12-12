namespace BeeEngine
{
    internal class ControlInfo
    {
        public Control control;
        public MouseEventHandler func;

        public ControlInfo(Control control, MouseEventHandler ev)
        {
            this.control = control;
            func = ev;
        }
    }
    public static class ControlsHelper
    {
        private static Point MouseDownPosition;
        private static List<ControlInfo> Controls = new List<ControlInfo>();

        /*public static void Movable(Control control, bool IsMovable = true)
        {
            if (IsMovable)
            {
                Controls.Add(new ControlInfo(control, (sender, e) => Control_MouseMoveMovable(sender, e, control)));
                control.MouseDown += MouseDownMovable;
                foreach (var controlInfo in Controls)
                {
                    if (controlInfo.control == control)
                    {
                        control.MouseMove += controlInfo.func;
                    }
                }
            }
            else
            {
                control.MouseDown += MouseDownMovable;
                foreach (var controlInfo in Controls)
                {
                    if (controlInfo.control == control)
                    {
                        control.MouseMove -= controlInfo.func;
                    }
                }
            }
        }*/
        public static void MoveOnMouseMove(this Control control, bool IsMovable = true)
        {
            if (IsMovable)
            {
                Controls.Add(new ControlInfo(control, (sender, e) => Control_MouseMoveMovable(sender, e, control)));
                control.MouseDown += MouseDownMovable;
                foreach (var controlInfo in Controls)
                {
                    if (controlInfo.control == control)
                    {
                        control.MouseMove += controlInfo.func;
                    }
                }
            }
            else
            {
                control.MouseDown += MouseDownMovable;
                foreach (var controlInfo in Controls)
                {
                    if (controlInfo.control == control)
                    {
                        control.MouseMove -= controlInfo.func;
                    }
                }
            }
        }
        
        public static void MoveAnotherOnMouseMove(this Control control,Control controlToMove, bool IsMovable = true)
        {
            if (IsMovable)
            {
                Controls.Add(new ControlInfo(control, (sender, e) => Control_MouseMoveMovable(sender, e, controlToMove)));
                control.MouseDown += MouseDownMovable;
                foreach (var controlInfo in Controls)
                {
                    if (controlInfo.control == control)
                    {
                        control.MouseMove += controlInfo.func;
                    }
                }
            }
            else
            {
                control.MouseDown += MouseDownMovable;
                foreach (var controlInfo in Controls)
                {
                    if (controlInfo.control == control)
                    {
                        control.MouseMove -= controlInfo.func;
                    }
                }
            }
        }
        /*public static void MovableParent(Control control,Control parent, bool IsMovable = true)
        {
            if (IsMovable)
            {
                Controls.Add(new ControlInfo(control, (sender, e) => Control_MouseMoveMovable(sender, e, parent)));
                control.MouseDown += MouseDownMovable;
                foreach (var controlInfo in Controls)
                {
                    if (controlInfo.control == control)
                    {
                        control.MouseMove += controlInfo.func;
                    }
                }
            }
            else
            {
                control.MouseDown += MouseDownMovable;
                foreach (var controlInfo in Controls)
                {
                    if (controlInfo.control == control)
                    {
                        control.MouseMove -= controlInfo.func;
                    }
                }
            }
        }*/

        private static void Control_MouseMoveMovable(object sender, MouseEventArgs e, Control control)
        {
            if (e.Button == MouseButtons.Left)
            {
                control.Left = e.X + control.Left - MouseDownPosition.X;
                control.Top = e.Y + control.Top - MouseDownPosition.Y;
            }
        }

        private static void MouseDownMovable(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MouseDownPosition = e.Location;
            }
        }
    }
}

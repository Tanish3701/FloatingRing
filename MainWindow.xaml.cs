using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace FloatingRing
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }

        readonly TrayWindow _tray = new TrayWindow();
        readonly DispatcherTimer _fadeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        Point _down;
        bool _pressed, _dragging;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            _fadeTimer.Tick += (_, _) =>
            {
                _fadeTimer.Stop();
                if (_tray.IsVisible) { _fadeTimer.Start(); return; }
                FadeTo(0.4);
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            NativeMethods.MakeNoActivate(this);

            var saved = Config.LoadPosition();
            var wa = SystemParameters.WorkArea;
            Left = saved?.X ?? wa.Right;
            Top = saved?.Y ?? wa.Height * 0.4;
            Snap();
            _fadeTimer.Start();
        }

        // ---------- click vs drag ----------
        void Root_Down(object s, MouseButtonEventArgs e)
        {
            _down = e.GetPosition(this);
            _pressed = true;
            _dragging = false;
        }

        void Root_Move(object s, MouseEventArgs e)
        {
            if (!_pressed || e.LeftButton != MouseButtonState.Pressed) return;
            var p = e.GetPosition(this);
            if (!_dragging && (Math.Abs(p.X - _down.X) > 4 || Math.Abs(p.Y - _down.Y) > 4))
            {
                _dragging = true;
                _tray.HideTray();
                FadeTo(1);
                DragMove();      // blocks until the mouse button is released
                _pressed = false;
                Snap();
            }
        }

        void Root_Up(object s, MouseButtonEventArgs e)
        {
            if (_pressed && !_dragging) ToggleTray();
            _pressed = false;
        }

        void Root_Enter(object s, MouseEventArgs e) { _fadeTimer.Stop(); FadeTo(1); }
        void Root_Leave(object s, MouseEventArgs e) { _fadeTimer.Stop(); _fadeTimer.Start(); }

        // ---------- helpers ----------
        void ToggleTray()
        {
            if (_tray.IsVisible) _tray.HideTray();
            else _tray.ShowNear(this);
        }

        /// <summary>Snap to the nearest left/right screen edge and remember the spot.</summary>
        void Snap()
        {
            var wa = ScreenHelper.WorkArea(this);
            bool leftHalf = Left + Width / 2 < wa.Left + wa.Width / 2;
            Left = leftHalf ? wa.Left + 4 : wa.Right - Width - 4;
            Top = Math.Max(wa.Top + 4, Math.Min(Top, wa.Bottom - Height - 4));
            Config.SavePosition(Left, Top);
        }

        void FadeTo(double value)
        {
            BeginAnimation(OpacityProperty, new DoubleAnimation(value, TimeSpan.FromMilliseconds(200)));
        }

        public void ToggleVisible()
        {
            if (IsVisible) { _tray.HideTray(); Hide(); }
            else { Show(); FadeTo(1); _fadeTimer.Start(); }
        }

        /// <summary>Hide the ring briefly (used so it isn't captured in screenshots).</summary>
        public void HideFor(int ms)
        {
            _tray.HideTray();
            Hide();
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
            t.Tick += (_, _) => { t.Stop(); Show(); };
            t.Start();
        }
    }
}

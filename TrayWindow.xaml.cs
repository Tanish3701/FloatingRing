using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace FloatingRing
{
    public partial class TrayWindow : Window
    {
        readonly DispatcherTimer _idle = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
        readonly DispatcherTimer _armTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        Button? _armed;

        public TrayWindow()
        {
            InitializeComponent();
            _idle.Tick += (_, _) => HideTray();
            _armTimer.Tick += (_, _) => Disarm();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            NativeMethods.MakeNoActivate(this);
        }

        public void ShowNear(Window ring)
        {
            Build();

            var content = (FrameworkElement)Content;
            content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var size = content.DesiredSize;

            var wa = ScreenHelper.WorkArea(ring);
            double cx = ring.Left + ring.Width / 2;
            double cy = ring.Top + ring.Height / 2;

            // open toward the middle of the screen
            double left = cx > wa.Left + wa.Width / 2
                ? ring.Left - size.Width + 8
                : ring.Left + ring.Width - 8;
            double top = cy - size.Height / 2;

            Left = Math.Max(wa.Left, Math.Min(left, wa.Right - size.Width));
            Top = Math.Max(wa.Top, Math.Min(top, wa.Bottom - size.Height));

            Show();
            _idle.Stop(); _idle.Start();
        }

        public void HideTray()
        {
            _idle.Stop();
            Disarm();
            Hide();
        }

        void Build()
        {
            Disarm();
            TileGrid.Children.Clear();
            foreach (var action in Config.LoadActions())
            {
                var stack = new StackPanel();
                stack.Children.Add(new TextBlock
                {
                    Text = action.Icon, FontSize = 26,
                    FontFamily = new FontFamily("Segoe UI Emoji"),
                    Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center
                });
                stack.Children.Add(new TextBlock
                {
                    Text = action.Name, FontSize = 11, Margin = new Thickness(0, 4, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                var btn = new Button
                {
                    Style = (Style)FindResource("Tile"),
                    Content = stack,
                    DataContext = action
                };
                btn.Click += Tile_Click;
                TileGrid.Children.Add(btn);
            }
        }

        async void Tile_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var action = (ActionItem)btn.DataContext;
            _idle.Stop(); _idle.Start();

            // dangerous actions need a second tap
            if (action.Confirm && !ReferenceEquals(_armed, btn))
            {
                Disarm();
                _armed = btn;
                btn.Tag = "confirm";
                Label(btn).Text = "Tap again";
                _armTimer.Stop(); _armTimer.Start();
                return;
            }

            HideTray();
            await Task.Delay(250);   // let the tray disappear before the action runs
            ActionRunner.Run(action);
        }

        void Disarm()
        {
            _armTimer.Stop();
            if (_armed != null)
            {
                _armed.Tag = null;
                if (_armed.DataContext is ActionItem a) Label(_armed).Text = a.Name;
                _armed = null;
            }
        }

        static TextBlock Label(Button b) => (TextBlock)((StackPanel)b.Content).Children[1];
    }
}

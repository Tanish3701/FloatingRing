using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace FloatingRing
{
    public partial class App : Application
    {
        Mutex? _mutex;
        System.Windows.Forms.NotifyIcon? _tray;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mutex = new Mutex(true, "FloatingRing.SingleInstance", out bool created);
            if (!created) { Shutdown(); return; }

            var ring = new MainWindow();
            ring.Show();
            SetupTrayIcon(ring);
        }

        void SetupTrayIcon(MainWindow ring)
        {
            using var bmp = new System.Drawing.Bitmap(32, 32);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(System.Drawing.Brushes.DodgerBlue, 2, 2, 28, 28);
                g.FillEllipse(System.Drawing.Brushes.White, 10, 10, 12, 12);
            }

            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("Show / hide ring", null, (_, _) => ring.ToggleVisible());
            menu.Items.Add("Edit actions (actions.json)", null, (_, _) =>
            {
                Config.LoadActions(); // makes sure the file exists
                Process.Start("notepad.exe", Config.ActionsPath);
            });

            var startup = new System.Windows.Forms.ToolStripMenuItem("Start with Windows")
            {
                CheckOnClick = true,
                Checked = StartupHelper.IsEnabled
            };
            startup.CheckedChanged += (_, _) => StartupHelper.Set(startup.Checked);
            menu.Items.Add(startup);

            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            menu.Items.Add("Exit", null, (_, _) =>
            {
                if (_tray != null) _tray.Visible = false;
                Shutdown();
            });

            _tray = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.Icon.FromHandle(bmp.GetHicon()),
                Text = "Floating Ring",
                ContextMenuStrip = menu,
                Visible = true
            };
            _tray.DoubleClick += (_, _) => ring.ToggleVisible();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_tray != null) { _tray.Visible = false; _tray.Dispose(); }
            base.OnExit(e);
        }
    }
}

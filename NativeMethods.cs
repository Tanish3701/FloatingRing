using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace FloatingRing
{
    public static class NativeMethods
    {
        const int GWL_EXSTYLE = -20;
        const int WS_EX_NOACTIVATE = 0x08000000;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
        [DllImport("user32.dll")] public static extern bool LockWorkStation();
        [DllImport("powrprof.dll")] public static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        /// <summary>Clicking the window won't steal focus; also hides it from Alt+Tab.</summary>
        public static void MakeNoActivate(Window w)
        {
            var h = new WindowInteropHelper(w).Handle;
            SetWindowLong(h, GWL_EXSTYLE, GetWindowLong(h, GWL_EXSTYLE) | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);
        }

        /// <summary>Press the keys in order, then release them in reverse (e.g. Win, D).</summary>
        public static void Tap(params byte[] keys)
        {
            foreach (var k in keys) keybd_event(k, 0, 0, UIntPtr.Zero);
            for (int i = keys.Length - 1; i >= 0; i--) keybd_event(keys[i], 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }

    public static class ScreenHelper
    {
        /// <summary>Work area (excludes taskbar) of the monitor the window is on, in WPF DIPs.</summary>
        public static Rect WorkArea(Window w)
        {
            double sx = 1, sy = 1;
            var src = PresentationSource.FromVisual(w);
            if (src?.CompositionTarget != null)
            {
                sx = src.CompositionTarget.TransformToDevice.M11;
                sy = src.CompositionTarget.TransformToDevice.M22;
            }
            var center = new System.Drawing.Point(
                (int)((w.Left + w.Width / 2) * sx), (int)((w.Top + w.Height / 2) * sy));
            var a = System.Windows.Forms.Screen.FromPoint(center).WorkingArea;
            return new Rect(a.Left / sx, a.Top / sy, a.Width / sx, a.Height / sy);
        }
    }

    public static class StartupHelper
    {
        const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string ValueName = "FloatingRing";

        public static bool IsEnabled
        {
            get
            {
                using var k = Registry.CurrentUser.OpenSubKey(KeyPath);
                return k?.GetValue(ValueName) != null;
            }
        }

        public static void Set(bool enable)
        {
            using var k = Registry.CurrentUser.OpenSubKey(KeyPath, true);
            if (k == null) return;
            if (enable) k.SetValue(ValueName, "\"" + Environment.ProcessPath + "\"");
            else k.DeleteValue(ValueName, false);
        }
    }
}

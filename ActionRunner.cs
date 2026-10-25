using System;
using System.Diagnostics;
using System.Windows;

namespace FloatingRing
{
    public static class ActionRunner
    {
        public static void Run(ActionItem a)
        {
            try
            {
                if (a.Type == "open") Open(a.Target, a.Args);
                else Builtin(a.Target);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Couldn't run \"{a.Name}\":\n{ex.Message}", "Floating Ring");
            }
        }

        static void Open(string target, string args = "") =>
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true, Arguments = args });

        static void Cmd(string exe, string args) =>
            Process.Start(new ProcessStartInfo(exe, args) { CreateNoWindow = true, UseShellExecute = false });

        static void Builtin(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "screenshot":
                    MainWindow.Instance?.HideFor(4000);   // keep the ring out of the shot
                    Open("ms-screenclip:");
                    break;
                case "lock":      NativeMethods.LockWorkStation(); break;
                case "sleep":     NativeMethods.SetSuspendState(false, false, false); break;
                case "shutdown":  Cmd("shutdown", "/s /t 0"); break;
                case "restart":   Cmd("shutdown", "/r /t 0"); break;
                case "signout":   Cmd("shutdown", "/l"); break;

                case "mute":      NativeMethods.Tap(0xAD); break;
                case "voldown":   NativeMethods.Tap(0xAE); break;
                case "volup":     NativeMethods.Tap(0xAF); break;
                case "next":      NativeMethods.Tap(0xB0); break;
                case "prev":      NativeMethods.Tap(0xB1); break;
                case "playpause": NativeMethods.Tap(0xB3); break;

                case "desktop":   NativeMethods.Tap(0x5B, 0x44); break;   // Win + D
                case "taskview":  NativeMethods.Tap(0x5B, 0x09); break;   // Win + Tab

                default: throw new InvalidOperationException("Unknown built-in action: " + name);
            }
        }
    }
}

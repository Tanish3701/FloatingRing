using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;

namespace FloatingRing
{
    public static class Config
    {
        static readonly JsonSerializerOptions Json = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        static string Dir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FloatingRing");

        public static string ActionsPath => Path.Combine(Dir, "actions.json");
        static string PosPath => Path.Combine(Dir, "position.txt");

        public static List<ActionItem> LoadActions()
        {
            Directory.CreateDirectory(Dir);
            if (!File.Exists(ActionsPath))
                File.WriteAllText(ActionsPath, JsonSerializer.Serialize(Defaults(), Json));
            try
            {
                return JsonSerializer.Deserialize<List<ActionItem>>(File.ReadAllText(ActionsPath)) ?? Defaults();
            }
            catch { return Defaults(); }
        }

        public static List<ActionItem> Defaults() => new List<ActionItem>
        {
            new() { Name = "Screenshot", Icon = "📸", Target = "screenshot" },
            new() { Name = "Lock",       Icon = "🔒", Target = "lock" },
            new() { Name = "Sleep",      Icon = "🌙", Target = "sleep" },
            new() { Name = "Restart",    Icon = "🔄", Target = "restart",  Confirm = true },
            new() { Name = "Shut down",  Icon = "🔌", Target = "shutdown", Confirm = true },
            new() { Name = "Mute",       Icon = "🔇", Target = "mute" },
            new() { Name = "Vol -",      Icon = "🔉", Target = "voldown" },
            new() { Name = "Vol +",      Icon = "🔊", Target = "volup" },
            new() { Name = "Desktop",    Icon = "🖥️", Target = "desktop" },
            new() { Name = "Settings",   Icon = "⚙️", Type = "open", Target = "ms-settings:" },
            new() { Name = "Task Mgr",   Icon = "📊", Type = "open", Target = "taskmgr.exe" },
            new() { Name = "Notepad",    Icon = "📝", Type = "open", Target = "notepad.exe" },
        };

        public static Point? LoadPosition()
        {
            try
            {
                var parts = File.ReadAllText(PosPath).Split(',');
                return new Point(
                    double.Parse(parts[0], CultureInfo.InvariantCulture),
                    double.Parse(parts[1], CultureInfo.InvariantCulture));
            }
            catch { return null; }
        }

        public static void SavePosition(double x, double y)
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(PosPath,
                    x.ToString(CultureInfo.InvariantCulture) + "," + y.ToString(CultureInfo.InvariantCulture));
            }
            catch { /* not critical */ }
        }
    }
}

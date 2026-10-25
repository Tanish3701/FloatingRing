namespace FloatingRing
{
    /// <summary>
    /// Type "builtin": Target is one of screenshot, lock, sleep, restart, shutdown, signout,
    ///                 mute, volup, voldown, playpause, next, prev, desktop, taskview.
    /// Type "open"   : Target is an exe path, folder, file, or URI (ms-settings:, https://...).
    ///                 Args is optional.
    /// </summary>
    public class ActionItem
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Type { get; set; } = "builtin";
        public string Target { get; set; } = "";
        public string Args { get; set; } = "";
        public bool Confirm { get; set; }
    }
}

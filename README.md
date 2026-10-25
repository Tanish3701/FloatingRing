# Floating Ring

A draggable floating button for Windows (like AssistiveTouch). Click it to open a tray of shortcuts.
Fully offline: every action is a local system call.

## Run
Requires the .NET 8 SDK on Windows.

    dotnet run

## Build a single exe
    dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true

## Use
- Click the ring: open/close the tray. Drag it: it snaps to the nearest left/right edge.
- Restart/Shut down need a second tap ("Tap again" in red).
- Tray icon (bottom-right of taskbar): show/hide, edit actions, start with Windows, exit.
- Edit actions: tray icon > "Edit actions", or %AppData%\FloatingRing\actions.json
  (changes apply next time you open the tray, no restart needed).

## actions.json
    { "Name": "VS Code", "Icon": "💻", "Type": "open", "Target": "C:\\Path\\To\\Code.exe" }
    { "Name": "Downloads", "Icon": "📁", "Type": "open", "Target": "shell:Downloads" }
    { "Name": "Next song", "Icon": "⏭️", "Type": "builtin", "Target": "next" }
    Add "Confirm": true to require a second tap.

Built-in targets: screenshot, lock, sleep, restart, shutdown, signout, mute, volup, voldown,
playpause, next, prev, desktop, taskview

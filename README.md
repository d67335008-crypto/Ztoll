# Ztoll

Dynamic Island–style floating panel for **Windows 10 / 11** (WPF, .NET 8).
Ztoll is a Windows port inspired by [Atoll](https://github.com/Ebullioscopic/Atoll) 2.3.3 (macOS).
Licensed under **GPL-3.0** — see [LICENSE](LICENSE) and [NOTICE](NOTICE).

## Stage 1 (this release): what works

| Feature | Details |
|---|---|
| Island | Borderless, topmost, click-through outside the pill, never steals focus, hidden from Alt+Tab |
| Animations | Spring-style expand, smooth collapse, animated corner radius, crossfade of contents, hover scaling on buttons, animated switches. Speed is adjustable |
| Interaction | Expand on hover and/or click, configurable hover and auto-collapse delays |
| Clock & date | 12/24-hour, optional seconds, localized date |
| Battery | Real data from Windows: percent, charging, time remaining; "AC" on desktop PCs |
| Quick tools | Settings, dark/light toggle, screenshot (Windows Snipping Tool), lock PC, collapse, quit |
| Tray icon | Expand/collapse, show/hide, toggle theme, settings, quit |
| Global hotkey | Default `Ctrl+Alt+Z`, recordable in Settings, conflict detection |
| Themes | System (follows Windows) / Dark / Light, live switching |
| Settings | Appearance, behavior, clock/battery, hotkey, launch at sign-in, reset to defaults, license links |
| DPI | Per-monitor-v2 aware, pixel-exact top-center positioning on the primary display |

Planned next stages (not yet implemented — nothing in the UI pretends otherwise):
2. Media (Windows SMTC), CPU / RAM / GPU / network stats
3. Timers & stopwatch, clipboard history, calendar
4. Shelf, notes, OSD, webcam, Bluetooth and the rest

macOS-only features (MacBook notch, MediaRemote, macOS Focus, Lock Screen widgets) get the closest Windows
equivalent where one exists.

## Download (built by GitHub Actions)

Every push builds on a Windows runner. Open **Actions → latest run → Artifacts**:

* `Ztoll-Setup` → **Ztoll Setup.exe** (installer; no .NET needed)
* `Ztoll-exe` → **Ztoll.exe** (portable, self-contained single file) + LICENSE/NOTICE

Pushing a tag such as `v0.1.0` additionally publishes both files on the **Releases** page.

## Build it yourself

Requirements: Windows 10/11, [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0),
optionally [Inno Setup 6](https://jrsoftware.org/isinfo.php) for the installer.

```powershell
dotnet test tests/Ztoll.Tests/Ztoll.Tests.csproj
dotnet publish src/Ztoll/Ztoll.csproj -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" installer\Ztoll.iss   # -> dist\Ztoll Setup.exe
```

For development just open `Ztoll.sln` in Visual Studio 2022 and press F5.

## Using Ztoll

* Move the pointer to the pill at the top of the screen (or click it, or press `Ctrl+Alt+Z`).
* Right-click the tray icon for the menu; double-click it for Settings.
* Settings are stored in `%AppData%\Ztoll\settings.json`.
* Uninstall via Windows *Installed apps*; portable `Ztoll.exe` can simply be deleted
  (disable "Launch at sign-in" first).

## Windows SmartScreen note

Builds are not code-signed, so Windows may show "Windows protected your PC".
Click **More info → Run anyway**. Code signing needs a paid certificate.

## Legal

Ztoll is not affiliated with Atoll, boring.notch or Apple Inc. "Dynamic Island" is a trademark of Apple Inc.
The Ztoll icon is original artwork; no Atoll artwork is used.

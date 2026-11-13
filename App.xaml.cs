// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System;
using System.Threading;
using System.Windows;
using Ztoll.Services;
using Ztoll.Views;

namespace Ztoll
{
    public partial class App : System.Windows.Application
    {
        public const string MutexName = "Ztoll.SingleInstance.7F3A9C";

        private Mutex? _mutex;
        private TrayService? _tray;
        private SettingsWindow? _settingsWindow;

        public static App Instance => (App)System.Windows.Application.Current;

        public SettingsService Settings { get; private set; } = null!;
        public ThemeService Theme { get; private set; } = null!;
        public HotkeyService Hotkeys { get; private set; } = null!;
        public MainWindow Island { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _mutex = new Mutex(true, MutexName, out bool createdNew);
            if (!createdNew)
            {
                _mutex.Dispose();
                _mutex = null;
                Shutdown();
                return;
            }

            Settings = new SettingsService();
            Settings.Load();

            Theme = new ThemeService(Settings);
            Theme.Apply();

            Hotkeys = new HotkeyService();

            Island = new MainWindow();
            Island.Show(); // creates the window handle; the hotkey is attached in MainWindow.OnSourceInitialized

            _tray = new TrayService(this);

            Settings.Changed += ApplyHotkey;
            ApplyHotkey();
        }

        public void ApplyHotkey()
        {
            var s = Settings.Current;
            if (s.HotkeyEnabled)
            {
                Hotkeys.Register(s.HotkeyModifiers, s.HotkeyKey);
            }
            else
            {
                Hotkeys.Unregister();
            }
        }

        public void ShowSettings()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow();
                _settingsWindow.Closed += (_, _) => _settingsWindow = null;
                _settingsWindow.Show();
            }
            else
            {
                if (_settingsWindow.WindowState == WindowState.Minimized)
                {
                    _settingsWindow.WindowState = WindowState.Normal;
                }
                _settingsWindow.Activate();
            }
        }

        public void Quit()
        {
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _tray?.Dispose();
            Hotkeys?.Dispose();
            Theme?.Dispose();
            if (_mutex != null)
            {
                try { _mutex.ReleaseMutex(); } catch (ApplicationException) { }
                _mutex.Dispose();
            }
            base.OnExit(e);
        }
    }
}

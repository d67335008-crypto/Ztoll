// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Ztoll.Models;
using Ztoll.Services;

namespace Ztoll.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settings;
        private bool _loading = true; // true while the window is being built so initial control events are ignored

        public SettingsWindow()
        {
            _settings = App.Instance.Settings;
            InitializeComponent();

            VersionText.Text = "Version " + GetVersion();
            LoadFromSettings();

            App.Instance.Theme.ThemeChanged += OnThemeChanged;
        }

        private static string GetVersion()
        {
            string? v = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (string.IsNullOrEmpty(v)) return "0.0.0";
            int plus = v.IndexOf('+');
            return plus >= 0 ? v.Substring(0, plus) : v;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ApplyTitleBarTheme();
        }

        protected override void OnClosed(EventArgs e)
        {
            App.Instance.Theme.ThemeChanged -= OnThemeChanged;
            base.OnClosed(e);
        }

        private void OnThemeChanged()
        {
            ApplyTitleBarTheme();

            // Keep the segmented control in sync when the theme is toggled from the island or the tray.
            bool previous = _loading;
            _loading = true;
            try
            {
                ThemeMode mode = _settings.Current.Theme;
                ThemeSystem.IsChecked = mode == ThemeMode.System;
                ThemeDark.IsChecked = mode == ThemeMode.Dark;
                ThemeLight.IsChecked = mode == ThemeMode.Light;
            }
            finally
            {
                _loading = previous;
            }
        }

        private void ApplyTitleBarTheme()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero) return;
            int dark = App.Instance.Theme.IsDark ? 1 : 0;
            NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
        }

        // ---- load ------------------------------------------------------------

        private void LoadFromSettings()
        {
            _loading = true;
            try
            {
                AppSettings s = _settings.Current;

                ThemeSystem.IsChecked = s.Theme == ThemeMode.System;
                ThemeDark.IsChecked = s.Theme == ThemeMode.Dark;
                ThemeLight.IsChecked = s.Theme == ThemeMode.Light;

                OpacitySlider.Value = s.Opacity;
                SpeedSlider.Value = s.AnimationSpeed;
                OffsetSlider.Value = s.TopOffset;
                HoverDelaySlider.Value = s.HoverDelayMs;
                CollapseSlider.Value = s.AutoCollapseMs;

                HoverExpandSwitch.IsChecked = s.HoverExpand;
                ClickExpandSwitch.IsChecked = s.ClickExpand;
                Use24Switch.IsChecked = s.Use24Hour;
                SecondsSwitch.IsChecked = s.ShowSeconds;
                BatterySwitch.IsChecked = s.ShowBattery;
                HotkeySwitch.IsChecked = s.HotkeyEnabled;

                HotkeyBox.Text = HotkeyFormatter.Format(s.HotkeyModifiers, s.HotkeyKey);
                AutostartSwitch.IsChecked = AutostartService.IsEnabled();
                AutostartStatus.Text = "Starts Ztoll automatically when you sign in to Windows.";

                UpdateValueLabels();
                UpdateHotkeyStatus();
            }
            finally
            {
                _loading = false;
            }
        }

        private void UpdateValueLabels()
        {
            OpacityValue.Text = ((int)Math.Round(OpacitySlider.Value * 100)).ToString(CultureInfo.CurrentCulture) + "%";
            SpeedValue.Text = SpeedSlider.Value.ToString("0.0", CultureInfo.CurrentCulture) + "x";
            OffsetValue.Text = ((int)OffsetSlider.Value).ToString(CultureInfo.CurrentCulture) + " px";
            HoverDelayValue.Text = ((int)HoverDelaySlider.Value).ToString(CultureInfo.CurrentCulture) + " ms";
            CollapseValue.Text = ((int)CollapseSlider.Value).ToString(CultureInfo.CurrentCulture) + " ms";
        }

        private void UpdateHotkeyStatus()
        {
            if (!_settings.Current.HotkeyEnabled)
            {
                HotkeyStatus.Text = "Disabled.";
            }
            else if (App.Instance.Hotkeys.IsRegistered)
            {
                HotkeyStatus.Text = "Active. Click the field and press a new combination to change it.";
            }
            else
            {
                HotkeyStatus.Text = "This combination is already used by another program. Choose a different one.";
            }
        }

        // ---- handlers --------------------------------------------------------

        private void Theme_Checked(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            if (sender is RadioButton rb && rb.Tag is string tag && Enum.TryParse(tag, out ThemeMode mode))
            {
                _settings.Current.Theme = mode;
                Commit();
                App.Instance.Theme.Apply();
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading) return;
            _settings.Current.Opacity = Math.Round(e.NewValue, 2);
            Commit();
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading) return;
            _settings.Current.AnimationSpeed = Math.Round(e.NewValue, 1);
            Commit();
        }

        private void OffsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading) return;
            _settings.Current.TopOffset = (int)Math.Round(e.NewValue);
            Commit();
        }

        private void HoverDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading) return;
            _settings.Current.HoverDelayMs = (int)Math.Round(e.NewValue);
            Commit();
        }

        private void CollapseSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading) return;
            _settings.Current.AutoCollapseMs = (int)Math.Round(e.NewValue);
            Commit();
        }

        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            AppSettings s = _settings.Current;
            s.HoverExpand = HoverExpandSwitch.IsChecked == true;
            s.ClickExpand = ClickExpandSwitch.IsChecked == true;
            s.Use24Hour = Use24Switch.IsChecked == true;
            s.ShowSeconds = SecondsSwitch.IsChecked == true;
            s.ShowBattery = BatterySwitch.IsChecked == true;
            s.HotkeyEnabled = HotkeySwitch.IsChecked == true;
            Commit();
            UpdateHotkeyStatus();
        }

        private void AutostartSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            bool want = AutostartSwitch.IsChecked == true;
            if (!AutostartService.SetEnabled(want))
            {
                AutostartStatus.Text = "Could not change the startup entry for this user.";
            }
            else
            {
                AutostartStatus.Text = "Starts Ztoll automatically when you sign in to Windows.";
            }
            _loading = true;
            AutostartSwitch.IsChecked = AutostartService.IsEnabled();
            _loading = false;
        }

        private void Commit()
        {
            _settings.Commit();
            UpdateValueLabels();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _settings.ResetToDefaults();
            App.Instance.Theme.Apply();
            LoadFromSettings();
        }

        // ---- hotkey recorder ---------------------------------------------------

        private void HotkeyBox_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HotkeyBox.Text = "Press new shortcut\u2026";
        }

        private void HotkeyBox_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HotkeyBox.Text = HotkeyFormatter.Format(_settings.Current.HotkeyModifiers, _settings.Current.HotkeyKey);
        }

        private void HotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            switch (key)
            {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LWin:
                case Key.RWin:
                case Key.None:
                case Key.DeadCharProcessed:
                case Key.ImeProcessed:
                    return; // wait for a real key
            }

            if (key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                Keyboard.ClearFocus();
                return;
            }

            uint mods = (uint)Keyboard.Modifiers; // ModifierKeys values equal the Win32 MOD_* flags
            if (mods == 0)
            {
                HotkeyBox.Text = "Add Ctrl, Alt, Shift or Win";
                return;
            }

            int vk = KeyInterop.VirtualKeyFromKey(key);
            if (vk <= 0) return;

            _settings.Current.HotkeyModifiers = mods;
            _settings.Current.HotkeyKey = vk;
            _settings.Current.HotkeyEnabled = true;
            HotkeySwitch.IsChecked = true;
            Commit();
            UpdateHotkeyStatus();
            Keyboard.ClearFocus();
        }

        // ---- about links -------------------------------------------------------

        private static void OpenTarget(string target)
        {
            try
            {
                Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open:\n" + target + "\n\n" + ex.Message, "Ztoll",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static void OpenLocalFile(string fileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, fileName);
            if (File.Exists(path))
            {
                OpenTarget(path);
            }
            else
            {
                MessageBox.Show(fileName + " was not found next to Ztoll.exe.", "Ztoll",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenLicense_Click(object sender, RoutedEventArgs e) => OpenLocalFile("LICENSE.txt");
        private void OpenNotice_Click(object sender, RoutedEventArgs e) => OpenLocalFile("NOTICE.txt");
        private void OpenAtoll_Click(object sender, RoutedEventArgs e) => OpenTarget("https://github.com/Ebullioscopic/Atoll");
        private void OpenBoringNotch_Click(object sender, RoutedEventArgs e) => OpenTarget("https://github.com/TheBoredTeam/boring.notch");

        private void OpenSettingsFolder_Click(object sender, RoutedEventArgs e)
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ztoll");
            Directory.CreateDirectory(dir);
            OpenTarget(dir);
        }
    }
}

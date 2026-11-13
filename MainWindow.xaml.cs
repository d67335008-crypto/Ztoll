// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Ztoll.Models;
using Ztoll.Services;

namespace Ztoll.Views
{
    public partial class MainWindow : Window
    {
        private const double CompactWidth = 190;
        private const double CompactHeight = 36;
        private const double CompactRadius = 18;
        private const double ExpandedWidth = 440;
        private const double ExpandedHeight = 172;
        private const double ExpandedRadius = 32;

        public static readonly DependencyProperty IslandRadiusProperty = DependencyProperty.Register(
            nameof(IslandRadius), typeof(double), typeof(MainWindow),
            new PropertyMetadata(CompactRadius, (d, e) => ((MainWindow)d).OnIslandRadiusChanged((double)e.NewValue)));

        private readonly SettingsService _settings;
        private readonly DispatcherTimer _clockTimer;
        private readonly DispatcherTimer _hoverTimer;
        private readonly DispatcherTimer _collapseTimer;
        private bool _expanded;
        private int _animationVersion;

        public MainWindow()
        {
            _settings = App.Instance.Settings;
            InitializeComponent();

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (_, _) => RefreshClock();

            _hoverTimer = new DispatcherTimer();
            _hoverTimer.Tick += (_, _) =>
            {
                _hoverTimer.Stop();
                if (Island.IsMouseOver) Expand();
            };

            _collapseTimer = new DispatcherTimer();
            _collapseTimer.Tick += (_, _) =>
            {
                _collapseTimer.Stop();
                if (!Island.IsMouseOver) Collapse();
            };

            _batteryTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _batteryTimer.Tick += (_, _) => RefreshBattery();

            _settings.Changed += OnSettingsChanged;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

            ApplySettings();
            RefreshClock();
            RefreshBattery();
            _clockTimer.Start();
            _batteryTimer.Start();
        }

        private readonly DispatcherTimer _batteryTimer;

        public double IslandRadius
        {
            get => (double)GetValue(IslandRadiusProperty);
            set => SetValue(IslandRadiusProperty, value);
        }

        public bool IsExpanded => _expanded;

        // ---- window plumbing -------------------------------------------------

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int style = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE,
                style | NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW);

            App.Instance.Hotkeys.Attach(hwnd);
            App.Instance.Hotkeys.Pressed += OnHotkeyPressed;

            Reposition();
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            Dispatcher.BeginInvoke(new Action(Reposition), DispatcherPriority.Loaded);
        }

        protected override void OnClosed(EventArgs e)
        {
            _clockTimer.Stop();
            _batteryTimer.Stop();
            _hoverTimer.Stop();
            _collapseTimer.Stop();
            _settings.Changed -= OnSettingsChanged;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
            base.OnClosed(e);
        }

        private void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(Reposition));
        }

        /// <summary>Centers the window at the top of the primary screen (pixel-exact, DPI aware).</summary>
        private void Reposition()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero) return;

            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            System.Drawing.Rectangle bounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds
                                              ?? new System.Drawing.Rectangle(0, 0, 1920, 1080);

            int widthPx = (int)Math.Round(ActualWidth > 0 ? ActualWidth * dpi.DpiScaleX : Width * dpi.DpiScaleX);
            int x = bounds.Left + (bounds.Width - widthPx) / 2;
            int y = bounds.Top + (int)Math.Round(_settings.Current.TopOffset * dpi.DpiScaleY);

            NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0,
                NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);
        }

        // ---- settings --------------------------------------------------------

        private void OnSettingsChanged()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ApplySettings();
                RefreshClock();
                RefreshBattery();
            }));
        }

        private void ApplySettings()
        {
            AppSettings s = _settings.Current;
            Island.Opacity = s.Opacity;
            Reposition();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            CompactBattery.Visibility = s.ShowBattery ? Visibility.Visible : Visibility.Collapsed;
            BatteryChip.Visibility = s.ShowBattery ? Visibility.Visible : Visibility.Collapsed;
        }

        // ---- clock & battery -------------------------------------------------

        private void RefreshClock()
        {
            AppSettings s = _settings.Current;
            DateTime now = DateTime.Now;
            string format = s.Use24Hour
                ? (s.ShowSeconds ? "HH:mm:ss" : "HH:mm")
                : (s.ShowSeconds ? "h:mm:ss tt" : "h:mm tt");
            string text = now.ToString(format, CultureInfo.CurrentCulture);
            CompactTime.Text = text;
            ExpandedTime.Text = text;
            ExpandedDate.Text = now.ToString("dddd, d MMMM", CultureInfo.CurrentCulture);
        }

        private void RefreshBattery()
        {
            System.Windows.Forms.PowerStatus ps = System.Windows.Forms.SystemInformation.PowerStatus;
            bool hasBattery = (ps.BatteryChargeStatus & System.Windows.Forms.BatteryChargeStatus.NoSystemBattery) == 0
                              && ps.BatteryChargeStatus != System.Windows.Forms.BatteryChargeStatus.Unknown;
            double fraction = ps.BatteryLifePercent;
            bool plugged = ps.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online;

            if (hasBattery && (fraction < 0 || fraction > 1))
            {
                hasBattery = false; // percentage unknown
            }

            CompactBattery.Update(hasBattery, fraction, plugged);
            ExpandedBattery.Update(hasBattery, fraction, plugged);

            if (!hasBattery)
            {
                BatteryCaption.Text = "Power: plugged in";
            }
            else if (plugged)
            {
                BatteryCaption.Text = fraction >= 0.995 ? "Fully charged" : "Charging";
            }
            else
            {
                int secs = ps.BatteryLifeRemaining;
                if (secs > 0)
                {
                    TimeSpan t = TimeSpan.FromSeconds(secs);
                    BatteryCaption.Text = string.Format(CultureInfo.CurrentCulture, "{0}h {1:00}m left", (int)t.TotalHours, t.Minutes);
                }
                else
                {
                    BatteryCaption.Text = "On battery";
                }
            }
        }

        // ---- expand / collapse ----------------------------------------------

        public void Toggle()
        {
            if (!IsVisible) Show();
            if (_expanded) Collapse(); else Expand();
        }

        public void Expand()
        {
            _collapseTimer.Stop();
            if (_expanded) return;
            AnimateTo(true);
        }

        public void Collapse()
        {
            _hoverTimer.Stop();
            _collapseTimer.Stop();
            if (!_expanded) return;
            AnimateTo(false);
        }

        private void OnHotkeyPressed()
        {
            if (!IsVisible) Show();
            if (_expanded)
            {
                Collapse();
                return;
            }

            Expand();
            // Opened with the keyboard: close again after a few seconds unless the mouse is over the island.
            _collapseTimer.Interval = TimeSpan.FromSeconds(6);
            _collapseTimer.Start();
        }

        private void AnimateTo(bool expand)
        {
            _expanded = expand;
            int version = ++_animationVersion;

            double speed = Math.Clamp(_settings.Current.AnimationSpeed, 0.5, 2.0);
            Duration size = new Duration(TimeSpan.FromMilliseconds((expand ? 460 : 320) / speed));

            IEasingFunction sizeEase = expand
                ? new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
                : new CubicEase { EasingMode = EasingMode.EaseInOut };

            Island.BeginAnimation(WidthProperty,
                new DoubleAnimation(expand ? ExpandedWidth : CompactWidth, size) { EasingFunction = sizeEase });
            Island.BeginAnimation(HeightProperty,
                new DoubleAnimation(expand ? ExpandedHeight : CompactHeight, size) { EasingFunction = sizeEase });
            BeginAnimation(IslandRadiusProperty,
                new DoubleAnimation(expand ? ExpandedRadius : CompactRadius, size)
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });

            if (expand)
            {
                ExpandedPanel.Visibility = Visibility.Visible;
                ExpandedPanel.BeginAnimation(OpacityProperty, new DoubleAnimation(1, Ms(240, speed))
                {
                    BeginTime = TimeSpan.FromMilliseconds(110 / speed)
                });
                CompactPanel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, Ms(110, speed)));
            }
            else
            {
                var fadeOut = new DoubleAnimation(0, Ms(130, speed));
                fadeOut.Completed += (_, _) =>
                {
                    if (version == _animationVersion && !_expanded)
                    {
                        ExpandedPanel.Visibility = Visibility.Collapsed;
                    }
                };
                ExpandedPanel.BeginAnimation(OpacityProperty, fadeOut);
                CompactPanel.BeginAnimation(OpacityProperty, new DoubleAnimation(1, Ms(200, speed))
                {
                    BeginTime = TimeSpan.FromMilliseconds(120 / speed)
                });
            }
        }

        private static Duration Ms(double ms, double speed) => new Duration(TimeSpan.FromMilliseconds(ms / speed));

        private void OnIslandRadiusChanged(double radius)
        {
            if (Island != null)
            {
                Island.CornerRadius = new CornerRadius(radius);
            }
        }

        // ---- mouse -----------------------------------------------------------

        private void Island_MouseEnter(object sender, MouseEventArgs e)
        {
            _collapseTimer.Stop();
            if (!_expanded && _settings.Current.HoverExpand)
            {
                _hoverTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(1, _settings.Current.HoverDelayMs));
                _hoverTimer.Start();
            }
        }

        private void Island_MouseLeave(object sender, MouseEventArgs e)
        {
            _hoverTimer.Stop();
            if (_expanded)
            {
                _collapseTimer.Interval = TimeSpan.FromMilliseconds(_settings.Current.AutoCollapseMs);
                _collapseTimer.Start();
            }
        }

        private void Island_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_expanded && _settings.Current.ClickExpand)
            {
                Expand();
            }
        }

        // ---- quick tools -----------------------------------------------------

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            Collapse();
            App.Instance.ShowSettings();
        }

        private void BtnTheme_Click(object sender, RoutedEventArgs e) => App.Instance.Theme.Toggle();

        private void BtnSnip_Click(object sender, RoutedEventArgs e)
        {
            Collapse();
            try
            {
                Process.Start(new ProcessStartInfo("ms-screenclip:") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start the Windows screenshot tool:\n" + ex.Message, "Ztoll",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnLock_Click(object sender, RoutedEventArgs e)
        {
            Collapse();
            NativeMethods.LockWorkStation();
        }

        private void BtnCollapse_Click(object sender, RoutedEventArgs e) => Collapse();

        private void BtnQuit_Click(object sender, RoutedEventArgs e) => App.Instance.Quit();
    }
}

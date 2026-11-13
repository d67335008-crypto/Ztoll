// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System;
using System.Windows;
using Microsoft.Win32;
using Ztoll.Models;

namespace Ztoll.Services
{
    /// <summary>Swaps the Dark/Light resource dictionary and follows the Windows app theme.</summary>
    public sealed class ThemeService : IDisposable
    {
        private readonly SettingsService _settings;
        private ResourceDictionary? _active;

        public ThemeService(SettingsService settings)
        {
            _settings = settings;
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        public bool IsDark { get; private set; } = true;

        public event Action? ThemeChanged;

        public static bool SystemUsesDarkApps()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", false);
                object? value = key?.GetValue("AppsUseLightTheme");
                return !(value is int i && i == 1);
            }
            catch (Exception)
            {
                return true;
            }
        }

        public void Apply()
        {
            ThemeMode mode = _settings.Current.Theme;
            bool dark = mode == ThemeMode.System ? SystemUsesDarkApps() : mode == ThemeMode.Dark;

            var dict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Themes/" + (dark ? "Dark" : "Light") + ".xaml", UriKind.Absolute)
            };

            var merged = System.Windows.Application.Current.Resources.MergedDictionaries;
            if (_active != null) merged.Remove(_active);
            merged.Add(dict);
            _active = dict;

            IsDark = dark;
            ThemeChanged?.Invoke();
        }

        /// <summary>Switches between explicit Dark and Light based on what is currently shown.</summary>
        public void Toggle()
        {
            _settings.Current.Theme = IsDark ? ThemeMode.Light : ThemeMode.Dark;
            _settings.Commit();
            Apply();
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (_settings.Current.Theme != ThemeMode.System) return;
            if (e.Category != UserPreferenceCategory.General && e.Category != UserPreferenceCategory.VisualStyle) return;
            System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(Apply));
        }

        public void Dispose()
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        }
    }
}

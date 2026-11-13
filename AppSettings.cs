// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System;

namespace Ztoll.Models
{
    public enum ThemeMode
    {
        System,
        Dark,
        Light
    }

    /// <summary>User settings persisted as JSON in %AppData%\Ztoll\settings.json.</summary>
    public sealed class AppSettings
    {
        // Win32 modifier flags (identical to System.Windows.Input.ModifierKeys values).
        public const uint ModAlt = 0x1;
        public const uint ModControl = 0x2;
        public const uint ModShift = 0x4;
        public const uint ModWin = 0x8;

        public ThemeMode Theme { get; set; } = ThemeMode.System;

        public bool HoverExpand { get; set; } = true;
        public bool ClickExpand { get; set; } = true;
        public int HoverDelayMs { get; set; } = 120;
        public int AutoCollapseMs { get; set; } = 700;

        /// <summary>Island opacity, 0.5 - 1.0.</summary>
        public double Opacity { get; set; } = 1.0;

        /// <summary>Animation speed multiplier, 0.5 - 2.0 (higher is faster).</summary>
        public double AnimationSpeed { get; set; } = 1.0;

        /// <summary>Distance from the top edge of the screen in device-independent pixels.</summary>
        public int TopOffset { get; set; } = 6;

        public bool Use24Hour { get; set; } = true;
        public bool ShowSeconds { get; set; } = false;
        public bool ShowBattery { get; set; } = true;

        public bool HotkeyEnabled { get; set; } = true;
        public uint HotkeyModifiers { get; set; } = ModControl | ModAlt;
        public int HotkeyKey { get; set; } = 0x5A; // VK_Z

        /// <summary>Clamps every value into its valid range (protects against hand-edited files).</summary>
        public void Sanitize()
        {
            if (!Enum.IsDefined(typeof(ThemeMode), Theme)) Theme = ThemeMode.System;
            HoverDelayMs = Math.Clamp(HoverDelayMs, 0, 1000);
            AutoCollapseMs = Math.Clamp(AutoCollapseMs, 200, 5000);
            Opacity = double.IsNaN(Opacity) ? 1.0 : Math.Clamp(Opacity, 0.5, 1.0);
            AnimationSpeed = double.IsNaN(AnimationSpeed) ? 1.0 : Math.Clamp(AnimationSpeed, 0.5, 2.0);
            TopOffset = Math.Clamp(TopOffset, 0, 60);
            HotkeyModifiers &= (ModAlt | ModControl | ModShift | ModWin);
            if (HotkeyKey <= 0 || HotkeyKey > 0xFE || HotkeyModifiers == 0)
            {
                HotkeyModifiers = ModControl | ModAlt;
                HotkeyKey = 0x5A;
            }
        }
    }
}

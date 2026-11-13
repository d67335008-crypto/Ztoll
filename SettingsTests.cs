using Ztoll.Models;
using Ztoll.Services;
using Xunit;

namespace Ztoll.Tests
{
    public class SettingsTests
    {
        [Fact]
        public void Defaults_AreValid()
        {
            var s = new AppSettings();
            s.Sanitize();
            Assert.Equal(ThemeMode.System, s.Theme);
            Assert.Equal(AppSettings.ModControl | AppSettings.ModAlt, s.HotkeyModifiers);
            Assert.Equal(0x5A, s.HotkeyKey);
        }

        [Fact]
        public void Sanitize_ClampsOutOfRangeValues()
        {
            var s = new AppSettings
            {
                Opacity = 5,
                AnimationSpeed = -3,
                TopOffset = 9999,
                HoverDelayMs = -1,
                AutoCollapseMs = 1,
                HotkeyModifiers = 0,
                HotkeyKey = 0
            };
            s.Sanitize();
            Assert.Equal(1.0, s.Opacity);
            Assert.Equal(0.5, s.AnimationSpeed);
            Assert.Equal(60, s.TopOffset);
            Assert.Equal(0, s.HoverDelayMs);
            Assert.Equal(200, s.AutoCollapseMs);
            Assert.NotEqual(0u, s.HotkeyModifiers);
            Assert.NotEqual(0, s.HotkeyKey);
        }

        [Fact]
        public void Json_RoundTrip_PreservesValues()
        {
            var s = new AppSettings
            {
                Theme = ThemeMode.Light,
                Use24Hour = false,
                ShowSeconds = true,
                Opacity = 0.8,
                HotkeyModifiers = AppSettings.ModControl | AppSettings.ModShift,
                HotkeyKey = 0x4B
            };
            string json = SettingsService.Serialize(s);
            Assert.Contains("\"Light\"", json);

            AppSettings back = SettingsService.Deserialize(json);
            Assert.Equal(ThemeMode.Light, back.Theme);
            Assert.False(back.Use24Hour);
            Assert.True(back.ShowSeconds);
            Assert.Equal(0.8, back.Opacity);
            Assert.Equal(AppSettings.ModControl | AppSettings.ModShift, back.HotkeyModifiers);
            Assert.Equal(0x4B, back.HotkeyKey);
        }

        [Fact]
        public void Deserialize_InvalidJson_ReturnsDefaults()
        {
            AppSettings s = SettingsService.Deserialize("{ this is not json");
            Assert.Equal(ThemeMode.System, s.Theme);
        }

        [Fact]
        public void SaveAndLoad_UsesFileOnDisk()
        {
            string dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ztoll-test-" + System.Guid.NewGuid().ToString("N"));
            string file = System.IO.Path.Combine(dir, "settings.json");
            try
            {
                var svc = new SettingsService(file);
                svc.Current.Theme = ThemeMode.Dark;
                svc.Current.TopOffset = 12;
                svc.Save();

                var svc2 = new SettingsService(file);
                svc2.Load();
                Assert.Equal(ThemeMode.Dark, svc2.Current.Theme);
                Assert.Equal(12, svc2.Current.TopOffset);
            }
            finally
            {
                if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void HotkeyFormatter_FormatsModifiersAndKey()
        {
            Assert.Equal("Ctrl + Alt + Z", HotkeyFormatter.Format(AppSettings.ModControl | AppSettings.ModAlt, 0x5A));
            Assert.Equal("Ctrl + Shift + Win + K", HotkeyFormatter.Format(
                AppSettings.ModControl | AppSettings.ModShift | AppSettings.ModWin, 0x4B));
            Assert.Equal("Alt + 5", HotkeyFormatter.Format(AppSettings.ModAlt, 0x35));
        }
    }
}

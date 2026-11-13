// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System;
using Microsoft.Win32;

namespace Ztoll.Services
{
    /// <summary>Launch-at-login through HKCU\...\Run (no admin rights required).</summary>
    public static class AutostartService
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "Ztoll";

        public static bool IsEnabled()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, false);
            return key?.GetValue(ValueName) is string;
        }

        public static bool SetEnabled(bool enabled)
        {
            try
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey, true);
                if (enabled)
                {
                    string? exe = Environment.ProcessPath;
                    if (string.IsNullOrEmpty(exe)) return false;
                    key.SetValue(ValueName, "\"" + exe + "\" --background");
                }
                else
                {
                    key.DeleteValue(ValueName, false);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

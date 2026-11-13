// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System.Collections.Generic;
using System.Windows.Input;
using Ztoll.Models;

namespace Ztoll.Services
{
    /// <summary>Converts a Win32 modifier mask + virtual key into readable text, e.g. "Ctrl + Alt + Z".</summary>
    public static class HotkeyFormatter
    {
        public static string Format(uint modifiers, int virtualKey)
        {
            var parts = new List<string>();
            if ((modifiers & AppSettings.ModControl) != 0) parts.Add("Ctrl");
            if ((modifiers & AppSettings.ModAlt) != 0) parts.Add("Alt");
            if ((modifiers & AppSettings.ModShift) != 0) parts.Add("Shift");
            if ((modifiers & AppSettings.ModWin) != 0) parts.Add("Win");

            Key key = KeyInterop.KeyFromVirtualKey(virtualKey);
            string name = key.ToString();
            if (name.Length == 2 && name[0] == 'D' && char.IsDigit(name[1]))
            {
                name = name.Substring(1);
            }
            parts.Add(name);
            return string.Join(" + ", parts);
        }
    }
}

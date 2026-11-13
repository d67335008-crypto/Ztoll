// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System;
using System.Windows.Interop;

namespace Ztoll.Services
{
    /// <summary>Global hotkey via RegisterHotKey, delivered through an HwndSource hook.</summary>
    public sealed class HotkeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HotkeyId = 0x5A01;
        private const uint MOD_NOREPEAT = 0x4000;

        private HwndSource? _source;
        private bool _registered;

        public event Action? Pressed;

        /// <summary>True when the OS accepted the current hotkey registration.</summary>
        public bool IsRegistered => _registered;

        public void Attach(IntPtr hwnd)
        {
            _source = HwndSource.FromHwnd(hwnd);
            _source?.AddHook(WndProc);
        }

        public bool Register(uint modifiers, int virtualKey)
        {
            Unregister();
            if (_source == null) return false;
            _registered = NativeMethods.RegisterHotKey(_source.Handle, HotkeyId, modifiers | MOD_NOREPEAT, (uint)virtualKey);
            return _registered;
        }

        public void Unregister()
        {
            if (_registered && _source != null)
            {
                NativeMethods.UnregisterHotKey(_source.Handle, HotkeyId);
            }
            _registered = false;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HotkeyId)
            {
                handled = true;
                Pressed?.Invoke();
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Unregister();
            _source?.RemoveHook(WndProc);
            _source = null;
        }
    }
}

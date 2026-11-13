// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System;
using System.Drawing;
using System.IO;
using System.Windows;

namespace Ztoll.Services
{
    /// <summary>Notification-area icon with the Ztoll context menu.</summary>
    public sealed class TrayService : IDisposable
    {
        private readonly App _app;
        private readonly System.Windows.Forms.NotifyIcon _icon;
        private readonly System.Windows.Forms.ToolStripMenuItem _visibilityItem;
        private readonly Icon? _iconResource;

        public TrayService(App app)
        {
            _app = app;

            var menu = new System.Windows.Forms.ContextMenuStrip();

            var expand = new System.Windows.Forms.ToolStripMenuItem("Expand / collapse island");
            expand.Click += (_, _) => _app.Island.Toggle();

            _visibilityItem = new System.Windows.Forms.ToolStripMenuItem("Hide island");
            _visibilityItem.Click += (_, _) => ToggleVisibility();

            var theme = new System.Windows.Forms.ToolStripMenuItem("Toggle dark / light theme");
            theme.Click += (_, _) => _app.Theme.Toggle();

            var settings = new System.Windows.Forms.ToolStripMenuItem("Ztoll Settings\u2026");
            settings.Click += (_, _) => _app.ShowSettings();

            var quit = new System.Windows.Forms.ToolStripMenuItem("Quit Ztoll");
            quit.Click += (_, _) => _app.Quit();

            menu.Items.Add(expand);
            menu.Items.Add(_visibilityItem);
            menu.Items.Add(theme);
            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            menu.Items.Add(settings);
            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            menu.Items.Add(quit);
            menu.Opening += (_, _) =>
            {
                _visibilityItem.Text = _app.Island.IsVisible ? "Hide island" : "Show island";
            };

            _iconResource = LoadIcon();
            _icon = new System.Windows.Forms.NotifyIcon
            {
                Text = "Ztoll",
                Icon = _iconResource ?? SystemIcons.Application,
                ContextMenuStrip = menu,
                Visible = true
            };
            _icon.MouseClick += (_, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    _app.Island.Toggle();
                }
            };
            _icon.DoubleClick += (_, _) => _app.ShowSettings();
        }

        private void ToggleVisibility()
        {
            if (_app.Island.IsVisible)
            {
                _app.Island.Hide();
            }
            else
            {
                _app.Island.Show();
            }
        }

        private static Icon? LoadIcon()
        {
            try
            {
                var info = System.Windows.Application.GetResourceStream(
                    new Uri("pack://application:,,,/Assets/Ztoll.ico", UriKind.Absolute));
                if (info == null) return null;
                using Stream s = info.Stream;
                return new Icon(s, new System.Drawing.Size(32, 32));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            _icon.Visible = false;
            _icon.Dispose();
            _iconResource?.Dispose();
        }
    }
}

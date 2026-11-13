// Ztoll - Dynamic Island style panel for Windows.
// Licensed under the GNU General Public License v3.0. See LICENSE and NOTICE.
// Derived concept from Atoll (c) Atoll Contributors, which is derived from boring.notch.

using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ztoll.Views
{
    public partial class BatteryIndicator : UserControl
    {
        private const double MaxFillWidth = 24 - 2 * 1.4 - 2 * 1.6; // outline width minus border minus margin

        public BatteryIndicator()
        {
            InitializeComponent();
        }

        /// <summary>Updates the glyph and label. fraction is 0..1.</summary>
        public void Update(bool hasBattery, double fraction, bool pluggedIn)
        {
            if (!hasBattery)
            {
                Fill.Width = MaxFillWidth;
                Fill.SetResourceReference(Border.BackgroundProperty, "Good");
                Label.Text = "AC";
                return;
            }

            fraction = Math.Clamp(fraction, 0, 1);
            Fill.Width = Math.Max(1.5, MaxFillWidth * fraction);
            Label.Text = ((int)Math.Round(fraction * 100)).ToString() + "%" + (pluggedIn ? " \u26A1" : string.Empty);

            if (pluggedIn)
            {
                Fill.SetResourceReference(Border.BackgroundProperty, "Good");
            }
            else if (fraction <= 0.15)
            {
                Fill.SetResourceReference(Border.BackgroundProperty, "Danger");
            }
            else
            {
                Fill.SetResourceReference(Border.BackgroundProperty, "PrimaryText");
            }
        }
    }
}

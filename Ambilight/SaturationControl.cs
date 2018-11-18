using System;
using System.Windows.Forms;

namespace Ambilight
{
    public partial class SaturationControl : Form
    {
        private EventHandler valueChangedHandler;

        public SaturationControl(EventHandler valueChangedHandler, float saturation)
        {
            this.valueChangedHandler = valueChangedHandler;
            InitializeComponent();
            this.saturationBar.Value = (int)saturation;
        }

        private void saturationBar_ValueChanged(object sender, EventArgs e)
        {
            valueChangedHandler.Invoke(sender, e);
        }
    }
}
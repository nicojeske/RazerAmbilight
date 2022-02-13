using System;
using System.Windows.Forms;

namespace Ambilight
{
    public partial class SaturationControl : Form
    {
        private readonly EventHandler _valueChangedHandler;

        public SaturationControl(EventHandler valueChangedHandler, float saturation)
        {
            _valueChangedHandler = valueChangedHandler;
            InitializeComponent();
            saturationBar.Value = (int)saturation;
        }

        private void saturationBar_ValueChanged(object sender, EventArgs e)
        {
            _valueChangedHandler.Invoke(sender, e);
        }
    }
}
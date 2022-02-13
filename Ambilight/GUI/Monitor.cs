using System;
using System.Windows.Forms;
using Ambilight.Util;

namespace Ambilight.GUI
{
    public partial class Monitor : Form
    {
        public Monitor(EventHandler monitorChangedHandler, int actualMonitor)
        {
            InitializeComponent();
            for (var i = 0; i < Screen.AllScreens.Length; i++)
            {
                comboBox1.Items.Add((i + 1) + ": " + Screen.AllScreens[i].DeviceFriendlyName() + " (" + Screen.AllScreens[i].Bounds.Width + "*" + Screen.AllScreens[i].Bounds.Height + ")");
            }
            comboBox1.SelectedIndex = actualMonitor;
            comboBox1.SelectedIndexChanged += monitorChangedHandler;
        }
    }
}

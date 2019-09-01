using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ambilight.Util;

namespace Ambilight.GUI
{
    public partial class Monitor : Form
    {
        public Monitor(EventHandler MonitorChangedHandler,int actualMonitor)
        {
            InitializeComponent();
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                comboBox1.Items.Add((i + 1) + ": " + Screen.AllScreens[i].DeviceFriendlyName() + " (" + Screen.AllScreens[i].Bounds.Width + "*" + Screen.AllScreens[i].Bounds.Height + ")");
            }
            comboBox1.SelectedIndex = actualMonitor;
            comboBox1.SelectedIndexChanged += MonitorChangedHandler;
        }
    }
}

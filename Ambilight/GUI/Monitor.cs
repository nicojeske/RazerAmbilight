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

namespace Ambilight.GUI
{
    public partial class Monitor : Form
    {
        public Monitor(EventHandler MonitorChangedHandler,int actualMonitor)
        {
            InitializeComponent();
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                comboBox1.Items.Add((i+1)+": Resolution:("+Screen.AllScreens[i].Bounds.Width+"*"+ Screen.AllScreens[i].Bounds.Height+")");
            }
            comboBox1.SelectedIndex = actualMonitor;
            //Console.WriteLine(Screen.AllScreens[0].DeviceName);
            comboBox1.SelectedIndexChanged += MonitorChangedHandler;
        }
    }
}

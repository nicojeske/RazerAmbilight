using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Colore.Effects.Keyboard;

namespace Ambilight
{
    public partial class KeyboardSizeControl : Form
    {
        private readonly EventHandler _keyboardSizeChangedHandler;

        public KeyboardSizeControl(EventHandler valuesChangedEventHandler, int keyboardWidth, int keyboardHeight)
        {
            InitializeComponent();

            this.widthTxt.Text = keyboardWidth.ToString();
            this.heightTxt.Text = keyboardHeight.ToString();

            this.widthTxt.TextChanged += LocalValuesChangedEventHandler;
            this.heightTxt.TextChanged += LocalValuesChangedEventHandler;

            this._keyboardSizeChangedHandler = valuesChangedEventHandler;


        }

        private void LocalValuesChangedEventHandler(object sender, EventArgs e)
        {

            if (int.TryParse(widthTxt.Text, out var outWidth) && int.TryParse(heightTxt.Text, out var outHeight) && outWidth > 0 && outHeight > 0 && outWidth <= KeyboardConstants.MaxColumns && outHeight <= KeyboardConstants.MaxRows) 
            {
       
                this.saveBtn.Enabled = true;
            }
            else
            {
                this.saveBtn.Enabled = false;
            }
            
        }


        public int GetTxtWidth()
        {
            return Convert.ToInt32(this.widthTxt.Text);
        }

        public int GetTxtHeight()
        {
            return Convert.ToInt32(this.heightTxt.Text);
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            this._keyboardSizeChangedHandler.Invoke(this, null);
            this.saveBtn.Enabled = false;
        }

        private void defaultButtonClicked(object sender, EventArgs e)
        {
            this.widthTxt.Text = KeyboardConstants.MaxColumns.ToString();
            this.heightTxt.Text = KeyboardConstants.MaxRows.ToString();
            saveBtn_Click(this, null);
        }

        public void errorReport (String msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

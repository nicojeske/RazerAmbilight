using System;
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

            widthTxt.Text = keyboardWidth.ToString();
            heightTxt.Text = keyboardHeight.ToString();

            widthTxt.TextChanged += LocalValuesChangedEventHandler;
            heightTxt.TextChanged += LocalValuesChangedEventHandler;

            _keyboardSizeChangedHandler = valuesChangedEventHandler;
        }

        private void LocalValuesChangedEventHandler(object sender, EventArgs e)
        {
            saveBtn.Enabled = int.TryParse(widthTxt.Text, out var outWidth) &&
                              int.TryParse(heightTxt.Text, out var outHeight) && outWidth > 0 && outHeight > 0 &&
                              outWidth <= KeyboardConstants.MaxColumns && outHeight <= KeyboardConstants.MaxRows;
        }


        public int GetTxtWidth()
        {
            return Convert.ToInt32(widthTxt.Text);
        }

        public int GetTxtHeight()
        {
            return Convert.ToInt32(heightTxt.Text);
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            _keyboardSizeChangedHandler.Invoke(this, null);
            saveBtn.Enabled = false;
        }

        private void DefaultButtonClicked(object sender, EventArgs e)
        {
            widthTxt.Text = KeyboardConstants.MaxColumns.ToString();
            heightTxt.Text = KeyboardConstants.MaxRows.ToString();
            saveBtn_Click(this, null);
        }

        public void ErrorReport(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

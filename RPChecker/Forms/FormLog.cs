// ****************************************************************************
// Public Domain
// code from http://sourceforge.net/projects/gmkvextractgui/
// ****************************************************************************

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace RPChecker.Forms
{
    public partial class FormLog : Form
    {
        private ReSulT _result;


        public FormLog(ReSulT result)
        {
            _result = result;
            InitializeComponent();
            InitForm();
        }

        private void InitForm()
        {
            Text = $"RPChecker v{Assembly.GetExecutingAssembly().GetName().Version} -- Log";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            txtLog.Text = _result.Logs.ToString();
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            txtLog.Select(txtLog.TextLength + 1, 0);
            txtLog.ScrollToCaret();
            grpLog.Text = $"Log ({txtLog.Lines.LongLength})";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.UnicodeText, txtLog.SelectedText);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void frmLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // To avoid getting disposed
            e.Cancel = true;
            Hide();
        }
    }
}
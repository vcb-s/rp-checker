using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace RPChecker.Forms
{
    public partial class Form2 : Form
    {
        private readonly int _poi;
        public Form2()
        {
            InitializeComponent();
            //this.SizeChanged += new System.EventHandler(this.Form2_SizeChanged);
            //this.BackColor = Color.DimGray;// "#252525";
            _poi                 = new Random().Next(1, 5);
            FormBorderStyle      = FormBorderStyle.None;
            label1.Text          = AssemblyProduct;
            label2.Text          = $"Version {AssemblyVersion}";
            label3.Text          = System.IO.File.GetLastWriteTime(Application.ExecutablePath).ToString(CultureInfo.InvariantCulture);
            notifyIcon1.Visible  = false;
        }

        private static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private static string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return attributes.Length == 0 ? string.Empty : ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }
        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized) return;
            Hide();
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(1000, "具体作用开发中~", "现在完全没用啦", ToolTipIcon.Info);
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Visible             = true;
            WindowState         = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void CloseForm()
        {
            while (Opacity > 0)
            {
                Opacity -= 0.02;
                Thread.Sleep(20);
            }
            Close();
        }

        private void button1_Click(object sender, EventArgs e) { if(_poi == 1) { CloseForm(); } }
        private void button2_Click(object sender, EventArgs e) { if(_poi == 2) { CloseForm(); } }
        private void button3_Click(object sender, EventArgs e) { if(_poi == 3) { CloseForm(); } }
        private void button4_Click(object sender, EventArgs e) { if(_poi == 4) { CloseForm(); } }


        //from http://www.sukitech.com/?p=948
        private Point _startPoint;
        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            _startPoint = new Point(-e.X , -e.Y);
            //startPoint = new Point(-e.X + SystemInformation.FrameBorderSize.Width, -e.Y - SystemInformation.FrameBorderSize.Height);
        }

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Point mousePos = MousePosition;
            mousePos.Offset(_startPoint.X, _startPoint.Y);
            Location       = mousePos;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Thread.Sleep(20000);
            WindowState = FormWindowState.Minimized;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}

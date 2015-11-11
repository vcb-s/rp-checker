using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace RPChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public List<KeyValuePair<string, string>> FilePathsPair = new List<KeyValuePair<string, string>>();
        List<reSulT> fullData = new List<reSulT>();

        class reSulT
        {
            public List<KeyValuePair<int, double>> data;
            public string fileName;

            public reSulT(string fileName, List<KeyValuePair<int, double>> data)
            {
                this.fileName = fileName;
                this.data     = data;
            }
        }

        

        string generateLog(object arguments, bool value = true)
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "AVSMeter.exe";
                process.StartInfo.Arguments = "\"" + (string)arguments + "\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = value;
                process.StartInfo.RedirectStandardOutput = value;

                process.Start();
                string output = string.Empty;
                if (value)
                {
                    output = process.StandardOutput.ReadToEnd();
                }

                process.WaitForExit();
                process.Close();
                return output;
            }
        }

        void generateAVS(string file1, string file2, string logFile, string outputFile)
        {
            string Template = "MP_Pipeline(\"\"\"\r\nLWLibavVideoSource(\"%File1%\", stacked=True, format=\"yuv420p8\")\r\n### prefetch: 16, 0\r\n### ###\r\nsrc = last\r\nLWLibavVideoSource(\"%File2%\", stacked=True, format=\"yuv420p8\")\r\n### export clip: src\r\n### prefetch: 16, 0\r\n### ###\r\nCompare(last, src, \"YUV\", \"%LogFile%\")\r\n\"\"\")";
            if (comboBox3.SelectedIndex != 1)
            {
                byte[] btemp = File.ReadAllBytes(comboBox3.SelectedItem.ToString());
                string temp = convertMethod.GetUTF8String(btemp);
                if (temp.IndexOf("%File1%") > 0 &&
                    temp.IndexOf("%File2%") > 0 &&
                    temp.IndexOf("%LogFile%") > 0)
                {
                    Template = temp;
                }
                else
                {
                    throw new ArgumentException("无效的模板文件");
                }
            }
            Template = Regex.Replace(Template, "%File1%", file1);
            Template = Regex.Replace(Template, "%File2%", file2);
            Template = Regex.Replace(Template, "%LogFile%", logFile);
            File.WriteAllText(outputFile, Template, Encoding.Default);
        }



        static int Compare(KeyValuePair<int, double> a, KeyValuePair<int, double> b)
        {
            return a.Value.CompareTo(b.Value);
        }
        List<KeyValuePair<int, double>> analyseFile(string file1, string file2, string avsFile, string logFile)
        {
            List<KeyValuePair<int, double>> data = new List<KeyValuePair<int, double>>();
            try
            {
                generateAVS(file1, file2, logFile, avsFile);
                generateLog(avsFile, false);
                string[] context = File.ReadAllLines(logFile);
                for (int i = 6; !string.IsNullOrWhiteSpace(context[i]); i++)
                {
                    string str = Regex.Replace(context[i], @"\s+", ",");
                    string[] item = str.Split(',');
                    data.Add(new KeyValuePair<int, double>(int.Parse(item[1]), double.Parse(item[6])));
                }
                data.Sort(Compare);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return data;
        }


        int Threshold = 30;

        void updataGridView(reSulT info, double frameRate,bool clear = true, bool updataTime = false)
        {
            if (clear) { dataGridView1.Rows.Clear(); }
            int index = 0;
            foreach (var item in info.data)
            {
                if ((dataGridView1.RowCount < 450 || item.Value < Threshold || updataTime) && index < dataGridView1.RowCount)
                {
                    if (clear) { index = dataGridView1.Rows.Add(); }
                    TimeSpan temp = convertMethod.second2Time(item.Key / frameRate);
                    dataGridView1.Rows[index].Cells[0].Value = item.Key;
                    dataGridView1.Rows[index].Cells[1].Value = item.Value;
                    dataGridView1.Rows[index].Cells[2].Value = convertMethod.time2string(temp);
                    dataGridView1.Rows[index].DefaultCellStyle.BackColor = ((item.Value < Threshold) ? Color.FromArgb(233, 76, 60) : Color.FromArgb(46, 205, 112));
                    if (!clear) { ++index; }
                }
                if ((item.Value > Threshold && dataGridView1.RowCount >= 450) && !updataTime) { break; }
            }

        }


        Regex Rpath = new Regex(@".+\\(?<fileName>.*)");
        private void button1_Click(object sender, EventArgs e)
        {
            fullData.Clear();
            foreach (var item in FilePathsPair)
            {
                string avsFile = item.Value + ".avs";
                string logFile = item.Value + ".log";

                fullData.Add(new reSulT(item.Value, analyseFile(item.Key, item.Value, avsFile, logFile)));
                if (!checkBox1.Checked)
                {
                    File.Delete(avsFile);
                    File.Delete(logFile);
                    File.Delete(item.Key   + ".lwi");
                    File.Delete(item.Value + ".lwi");
                }
            }
            foreach (var item in fullData)
            {
                comboBox1.Items.Add(Rpath.Match(item.fileName).Groups["fileName"].Value);
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
                updataGridView(fullData[comboBox1.SelectedIndex], FrameRate[comboBox2.SelectedIndex]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FrmLoadFiles flf = new FrmLoadFiles(this);
            flf.Show();
        }
        double[] FrameRate = {24000 / 1001.0, 24000 / 1000.0,
                              25000 / 1000.0, 30000 / 1001.0,
                              50000 / 1000.0, 60000 / 1001.0 };
        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            updataGridView(fullData[comboBox1.SelectedIndex], FrameRate[comboBox2.SelectedIndex]);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //for (int index = 0; index <= 450; index = dataGridView1.Rows.Add());
            comboBox2.SelectedIndex = 0;
            comboBox3.Items.Add("Default");
            comboBox3.SelectedIndex = 0;
            DirectoryInfo current = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);
            foreach (var item in current.GetFiles())
            {
                if (item.Extension.ToLowerInvariant().EndsWith("avs"))
                {
                    comboBox3.Items.Add(item);
                }
            }
        }



        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                updataGridView(fullData[comboBox1.SelectedIndex], FrameRate[comboBox2.SelectedIndex],false,true);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Threshold = Convert.ToInt32(numericUpDown1.Value);
        }
    }
}

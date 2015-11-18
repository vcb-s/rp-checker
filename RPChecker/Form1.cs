using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
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
        readonly List<ReSulT> _fullData = new List<ReSulT>();

        class ReSulT
        {
            public readonly List<KeyValuePair<int, double>> Data;
            public readonly string FileName;

            public ReSulT(string fileName, List<KeyValuePair<int, double>> data)
            {
                FileName = fileName;
                Data     = data;
            }
        }


        static void GenerateLog(string arguments, bool value = true)
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName               = "AVSMeter.exe";
                process.StartInfo.Arguments              = $"\"{arguments}\"";
                process.StartInfo.UseShellExecute        = false;
                process.StartInfo.CreateNoWindow         = value;
                process.StartInfo.RedirectStandardOutput = value;

                process.Start();

                process.WaitForExit();
                process.Close();
            }
        }

        void GenerateAvs(string file1, string file2, string logFile, string outputFile)
        {
            string template = "MP_Pipeline(\"\"\"\r\nLWLibavVideoSource(\"%File1%\", stacked=True, format=\"yuv420p8\")\r\n### prefetch: 16, 0\r\n### ###\r\nsrc = last\r\nLWLibavVideoSource(\"%File2%\", stacked=True, format=\"yuv420p8\")\r\n### export clip: src\r\n### prefetch: 16, 0\r\n### ###\r\nCompare(last, src, \"YUV\", \"%LogFile%\")\r\n\"\"\")";
            if (comboBox3.SelectedItem.ToString() != "Default")
            {
                byte[] btemp = File.ReadAllBytes(comboBox3.SelectedItem.ToString());
                string temp = ConvertMethod.GetUTF8String(btemp);
                if (temp.IndexOf("%File1%"  , StringComparison.Ordinal) > 0 &&
                    temp.IndexOf("%File2%"  , StringComparison.Ordinal) > 0 &&
                    temp.IndexOf("%LogFile%", StringComparison.Ordinal) > 0)
                {
                    template = temp;
                }
                else
                {
                    throw new ArgumentException("无效的模板文件");
                }
            }
            template = Regex.Replace(template, "%File1%", file1);
            template = Regex.Replace(template, "%File2%", file2);
            template = Regex.Replace(template, "%LogFile%", logFile);
            File.WriteAllText(outputFile, template, Encoding.Default);
        }



        static int Compare(KeyValuePair<int, double> a, KeyValuePair<int, double> b)
        {
            return a.Value.CompareTo(b.Value);
        }
        List<KeyValuePair<int, double>> AnalyseFile(string file1, string file2, string avsFile, string logFile)
        {
            try
            {
                GenerateAvs(file1, file2, logFile, avsFile);
                GenerateLog(avsFile, false);
                var data =
                    File.ReadAllLines(logFile)
                        .Skip(6)
                        .TakeWhile(item => !string.IsNullOrEmpty(item))
                        .Select(item => Regex.Replace(item, @"\s+", @",").Split(','))
                        .Select(item => new KeyValuePair<int, double>(int.Parse(item[1]), double.Parse(item[6])))
                        .ToList();
                data.Sort(Compare);
                return data;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return null;
        }


        int _threshold = 30;

        void UpdataGridView(ReSulT info, double frameRate, bool clear = true, bool updataTime = false)
        {
            if (clear) { dataGridView1.Rows.Clear(); }
            int index = 0;
            foreach (var item in info.Data)
            {
                if ((dataGridView1.RowCount < 450 || item.Value < _threshold || updataTime) && index < dataGridView1.RowCount)
                {
                    if (clear) { index = dataGridView1.Rows.Add(); }
                    TimeSpan temp = ConvertMethod.Second2Time(item.Key / frameRate);
                    dataGridView1.Rows[index].Cells[0].Value = item.Key;
                    dataGridView1.Rows[index].Cells[1].Value = item.Value;
                    dataGridView1.Rows[index].Cells[2].Value = ConvertMethod.Time2String(temp);
                    dataGridView1.Rows[index].DefaultCellStyle.BackColor = ((item.Value < _threshold) ? Color.FromArgb(233, 76, 60) : Color.FromArgb(46, 205, 112));
                    if (!clear) { ++index; }
                }
                if ((item.Value > _threshold && dataGridView1.RowCount >= 450) && !updataTime) { break; }
            }

        }


        readonly Regex _rpath = new Regex(@".+\\(?<fileName>.*)");
        private void button1_Click(object sender, EventArgs e)
        {
            _fullData.Clear();
            foreach (var item in FilePathsPair)
            {
                string avsFile = item.Value + ".avs";
                string logFile = item.Value + ".log";

                _fullData.Add(new ReSulT(item.Value, AnalyseFile(item.Key, item.Value, avsFile, logFile)));
                if (checkBox1.Checked) continue;
                File.Delete(avsFile);
                File.Delete(logFile);
                File.Delete($"{item.Key}.lwi");
                File.Delete($"{item.Value}.lwi");
            }
            _fullData.ForEach(item => comboBox1.Items.Add(_rpath.Match(item.FileName).Groups["fileName"].Value));
            if (comboBox1.Items.Count <= 0) return;
            comboBox1.SelectedIndex = 0;
            UpdataGridView(_fullData[comboBox1.SelectedIndex], _frameRate[comboBox2.SelectedIndex]);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FrmLoadFiles flf = new FrmLoadFiles(this);
            flf.Show();
        }

        readonly double[] _frameRate = {24000 / 1001.0, 24000 / 1000.0,
                                        25000 / 1000.0, 30000 / 1001.0,
                                        50000 / 1000.0, 60000 / 1001.0 };
        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            UpdataGridView(_fullData[comboBox1.SelectedIndex], _frameRate[comboBox2.SelectedIndex]);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //for (int index = 0; index <= 450; index = dataGridView1.Rows.Add());
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            DirectoryInfo current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            current.GetFiles()
                .Where(item => item.Extension.ToLowerInvariant().EndsWith("avs"))
                .ToList()
                .ForEach(item => comboBox3.Items.Add(item));
        }



        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                UpdataGridView(_fullData[comboBox1.SelectedIndex], _frameRate[comboBox2.SelectedIndex], false, true);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _threshold = Convert.ToInt32(numericUpDown1.Value);
            if (_fullData == null) return;
            UpdataGridView(_fullData[comboBox1.SelectedIndex], _frameRate[comboBox2.SelectedIndex]);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Text;
using RPChecker.Util;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RPChecker.Forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public readonly List<KeyValuePair<string, string>> FilePathsPair = new List<KeyValuePair<string, string>>();
        private readonly List<ReSulT> _fullData = new List<ReSulT>();
        private readonly StringBuilder _erroeMessageBuilder = new StringBuilder();
        private bool _beginErrorRecord;
        private int _threshold = 30;

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = $"[VCB-Studio] RP Checker v{Assembly.GetExecutingAssembly().GetName().Version}";

            Point saved = ConvertMethod.String2Point(RegistryStorage.Load(@"Software\RPChecker", "location"));
            if (saved != new Point(-32000, -32000)) Location = saved;
            RegistryStorage.RegistryAddCount(@"Software\RPChecker\Statistics", @"Count");

            cbFPS.SelectedIndex     = 0;
            cbVpyFile.SelectedIndex = 0;
            DirectoryInfo current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            foreach (var item in current.GetFiles().Where(item => item.Extension.ToLowerInvariant() == ".vpy"))
            {
                cbVpyFile.Items.Add(item);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            FrmLoadFiles flf = new FrmLoadFiles(this);
            flf.Show();
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_erroeMessageBuilder.ToString()))
            {
                MessageBox.Show(_erroeMessageBuilder.ToString(), @"Message");
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            try
            {
                VsPipeProcess.Abort = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private readonly double[] _frameRate = { 24000/1001.0, 24, 25, 30000/1001.0, 50, 60000/1001.0 };

        private void cbFileList_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cbFileList.SelectedIndex < 0 || cbFileList.SelectedIndex > _fullData.Count) return;
            UpdataGridView(_fullData[cbFileList.SelectedIndex], _frameRate[cbFPS.SelectedIndex]);
        }

        private void cbFPS_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbFileList.SelectedIndex != -1)
            {
                UpdataGridView(_fullData[cbFileList.SelectedIndex], _frameRate[cbFPS.SelectedIndex], false, true);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _threshold = Convert.ToInt32(numericUpDown1.Value);
            if (_fullData == null) return;
            UpdataGridView(_fullData[cbFileList.SelectedIndex], _frameRate[cbFPS.SelectedIndex]);
        }

        private void cbFileList_MouseEnter(object sender, EventArgs e) => toolTip1.Show(cbFileList.SelectedItem?.ToString(), cbFileList);

        private void cbFileList_MouseLeave(object sender, EventArgs e) => toolTip1.RemoveAll();


        private void UpdataGridView(ReSulT info, double frameRate, bool clear = true, bool updataTime = false)
        {
            if (clear) { dataGridView1.Rows.Clear(); }
            int index = 0;
            foreach (var item in info.Data)
            {
                if ((dataGridView1.RowCount < 450 || item.Value < _threshold || updataTime) && index < dataGridView1.RowCount)
                {
                    if (clear) { index = dataGridView1.Rows.Add(); }
                    TimeSpan temp = ConvertMethod.Second2Time(item.Key/frameRate);
                    dataGridView1.Rows[index].Cells[0].Value = item.Key;
                    dataGridView1.Rows[index].Cells[1].Value = $"{item.Value:F4}";
                    dataGridView1.Rows[index].Cells[2].Value = ConvertMethod.Time2String(temp);
                    dataGridView1.Rows[index].DefaultCellStyle.BackColor = item.Value < _threshold
                        ? Color.FromArgb(233, 76, 60)
                        : Color.FromArgb(46, 205, 112);
                    if (!clear) { ++index; }
                }
                if (item.Value > _threshold && dataGridView1.RowCount >= 450 && !updataTime) { break; }
            }
            Debug.WriteLine($"DataGridView with {index} lines");
        }

        //private static int Compare(KeyValuePair<int, double> a, KeyValuePair<int, double> b) => a.Value.CompareTo(b.Value);
        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            _fullData.Clear();
            _erroeMessageBuilder.Clear();
            cbFileList.Items.Clear();
            foreach (var item in FilePathsPair)
            {
                string vsFile = $"{item.Value}.vpy";
                try
                {
                    AnalyseFile(item.Key, item.Value, vsFile, cbVpyFile.SelectedItem.ToString());
                    //foreach (var psnr in _rawPsnrData.Select(GetPsnr).Where(psnr => psnr != null)) { _tempData.Add((KeyValuePair<int,double>)psnr); }

                    _tempData.Sort((a, b) => a.Value.CompareTo(b.Value));
                    var result = new ReSulT
                    {
                        FileName = item.Value,
                        Data     = _tempData
                    };
                    _fullData.Add(result);
                    RegistryStorage.RegistryAddCount(@"Software\RPChecker\Statistics", @"CheckedCount");
                    if (checkBox1.Checked) continue;
                    try
                    {
                        File.Delete(vsFile);
                        File.Delete($"{item.Key}.lwi");
                        File.Delete($"{item.Value}.lwi");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, @"PRChecker Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, @"PRChecker ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            _fullData.ForEach(item => cbFileList.Items.Add(Path.GetFileName(item.FileName) ?? ""));
            if (cbFileList.Items.Count <= 0) return;
            cbFileList.SelectedIndex = 0;
            UpdataGridView(_fullData[cbFileList.SelectedIndex], _frameRate[cbFPS.SelectedIndex]);
        }

        //private int count = 0;
        private void UpdateProgress(string progress)
        {
            lbError.Text = progress;
            if (progress == "Script evaluation failed:")
            {
                _beginErrorRecord = true;
            }
            if (_beginErrorRecord)
            {
                _erroeMessageBuilder.Append(progress + Environment.NewLine);
                //Debug.WriteLine($"{++count} {progress} [{Thread.CurrentThread.ManagedThreadId}]");
                if (progress == "ImportError: No module named 'mvsfunc'")
                {
                    MessageBox.Show(caption: @"PRChecker ERROR", icon: MessageBoxIcon.Error, buttons: MessageBoxButtons.OK,
                        text: $"尚未正确放置mawen菊苣的滤镜库‘mvsfunc’{Environment.NewLine}大概的位置是在Python35\\Lib\\site-packages");
                }
            }

            var value = Regex.Match(progress, @"Frame: (?<done>\d+)/(?<undo>\d+)");
            if (!value.Success) return;
            var done = double.Parse(value.Groups["done"].Value);
            var undo = double.Parse(value.Groups["undo"].Value);
            if (value.Success && done < undo)
            {
                progressBar1.Value = (int)Math.Floor(done / undo * 100);
            }
            Application.DoEvents();
        }

        private delegate void UpdateProgressDelegate(string progress);

        private void ProgressUpdated(string progress)
        {
            if (string.IsNullOrEmpty(progress))return;
            Invoke(new UpdateProgressDelegate(UpdateProgress), progress);
        }

        private volatile List<KeyValuePair<int, double>> _tempData = new List<KeyValuePair<int, double>>();

        //private volatile List<string> _rawPsnrData;

        private void UpdatePsnr(string data)
        {
            //_rawPsnrData.Add(data);
            var rawData = Regex.Match(data, @"(?<fram>\d+) (?<PSNR>[-+]?[0-9]*\.?[0-9]+)");
            if (!rawData.Success) return;
            _tempData.Add(new KeyValuePair<int, double>(int.Parse(rawData.Groups["fram"].Value), double.Parse(rawData.Groups["PSNR"].Value)));
        }

        private KeyValuePair<int, double>? GetPsnr(string data)
        {
            var rawData = Regex.Match(data, @"(?<fram>\d+) (?<PSNR>[-+]?[0-9]*\.?[0-9]+)");
            if (!rawData.Success) return null;
            return new KeyValuePair<int, double>(int.Parse(rawData.Groups["fram"].Value),
                double.Parse(rawData.Groups["PSNR"].Value));
        }

        private delegate void UpdatePsnrDelegate(string data);

        private void PsnrUpdated(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            Invoke(new UpdatePsnrDelegate(UpdatePsnr), data);
        }

        private bool Enable
        {
            set
            {
                btnAnalyze.Enabled     = value;
                btnLoad.Enabled        = value;
                btnLog.Enabled         = value;
                btnChart.Enabled       = value;
                cbFileList.Enabled     = value;
                cbFPS.Enabled          = value;
                cbVpyFile.Enabled      = value;
                checkBox1.Enabled      = value;
                numericUpDown1.Enabled = value;
                btnAbort.Enabled       = !value;
            }
        }

        private void AnalyseFile(string file1, string file2, string vsFile, string selectedFile)
        {
            _tempData = new List<KeyValuePair<int, double>>();
            //_rawPsnrData = new List<string>();
            try
            {
                _beginErrorRecord  = false;
                Enable             = false;
                lbError.Text       = @"生成lwi文件中……";
                progressBar1.Value = 0;
                Application.DoEvents();

                ConvertMethod.GenerateVpyFile(file1, file2, vsFile, selectedFile);

                _erroeMessageBuilder.Append("---" + vsFile + "---" + Environment.NewLine);
                var vsThread = new Thread(VsPipeProcess.GenerateLog);
                VsPipeProcess.ProgressUpdated += ProgressUpdated;
                VsPipeProcess.PsnrUpdated     += PsnrUpdated;
                vsThread.Start(vsFile);

                while (vsThread.ThreadState != System.Threading.ThreadState.Stopped) Application.DoEvents();
                if (VsPipeProcess.VsPipeNotFind)
                {
                    lbError.Text = @"无可用vspipe";
                    throw new Exception(lbError.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"RPChecker Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                VsPipeProcess.ProgressUpdated -= ProgressUpdated;
                VsPipeProcess.PsnrUpdated     -= PsnrUpdated;
                progressBar1.Value             = 100;
                Enable                         = true;
                Refresh();
                Application.DoEvents();
            }
        }

        private void btnChart_Click(object sender, EventArgs e)
        {
            if (cbFileList.SelectedIndex == -1) return;
            FrmChart chart = new FrmChart(_fullData[cbFileList.SelectedIndex], _threshold);
            chart.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            RegistryStorage.Save(Location.ToString(), @"Software\RPChecker", "Location");
        }

        private readonly int[] _poi = { 0, 10 };

        private void progressBar1_Click(object sender, EventArgs e)
        {
            ++_poi[0];
            if (_poi[0] >= _poi[1])
            {
                Form2 version = new Form2();
                version.Show();
                _poi[0] = 00;
                _poi[1] += 10;
            }
            if (_poi[0] < 3 && _poi[1] == 10)
            {
                MessageBox.Show(@"Something happened", @"Something happened");
            }
        }
    }
    public class ReSulT
    {
        public List<KeyValuePair<int, double>> Data { get; set; }
        public string FileName { get; set; }
    }
}

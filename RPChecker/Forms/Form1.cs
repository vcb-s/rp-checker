using System;
using System.IO;
using System.Linq;
using RPChecker.Util;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using RPChecker.Properties;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using RPChecker.Util.FilterProcess;
using System.Text.RegularExpressions;

namespace RPChecker.Forms
{
    public partial class Form1 : Form
    {
        #region Form init
        public Form1()
        {
            InitializeComponent();
            AddCommand();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateText();

            var saved = ToolKits.String2Point(RegistryStorage.Load(@"Software\RPChecker", "location"));
            if (saved != new Point(-32000, -32000)) Location = saved;
            this.NormalizePosition();
            RegistryStorage.RegistryAddCount(@"Software\RPChecker\Statistics", @"Count");

            cbFPS.SelectedIndex = 0;
            cbVpyFile.SelectedIndex = 0;
            var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            cbVpyFile.Items.AddRange(current.GetFiles("*.vpy").ToArray<object>());
            btnAnalyze.Enabled = false;

            Updater.CheckUpdateWeekly("RPChecker");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            RegistryStorage.Save(Location.ToString(), @"Software\RPChecker", "Location");
        }
        #endregion

        public readonly List<(string src, string opt)> FilePathsPair = new List<(string src, string opt)>();
        private readonly List<ReSulT> _fullData = new List<ReSulT>();
        private int _threshold = 30;
        private readonly double[] _frameRate = { 24000 / 1001.0, 24, 25, 30000 / 1001.0, 50, 60000 / 1001.0 };
        private IProcess _coreProcess = new FFmpegPSNRProcess();

        private ReSulT CurrentData => _fullData[cbFileList.SelectedIndex];
        private double FrameRate   => _frameRate[cbFPS.SelectedIndex];

        #region SystemMenu
        private SystemMenu _systemMenu;

        private void UpdateText()
        {
            label1.Text = _coreProcess.ValueText;
            cbVpyFile.Enabled = _coreProcess is VsPipePSNRProcess;
            Text = $"[VCB-Studio] RP Checker v{Assembly.GetExecutingAssembly().GetName().Version} [{_coreProcess.Title}][{(UseOriginPath ? "ORG" : "LINK")}]";
            _threshold = _coreProcess.Threshold;
            numericUpDown1.Value = _threshold;
        }

        private void AddCommand()
        {
            _systemMenu = new SystemMenu(this);
            _systemMenu.AddCommand("检查更新(&U)", Updater.CheckUpdate, true);
            _systemMenu.AddCommand("使用PSNR(VS)", () =>
            {
                _coreProcess = _coreProcess as VsPipePSNRProcess ?? new VsPipePSNRProcess();
                UpdateText();
            }, true);
            _systemMenu.AddCommand("使用PSNR(FF)", () =>
            {
                _coreProcess = _coreProcess as FFmpegPSNRProcess ?? new FFmpegPSNRProcess();
                UpdateText();
            }, false);
            _systemMenu.AddCommand("使用SSIM(FF)", () =>
            {
                _coreProcess = _coreProcess as FFmpegSSIMProcess ?? new FFmpegSSIMProcess();
                UpdateText();
            }, false);
            _systemMenu.AddCommand("使用原始路径", () =>
            {
                _useOriginPath = true;
                UpdateText();
            }, true);
        }

        protected override void WndProc(ref Message msg)
        {
            base.WndProc(ref msg);

            // Let it know all messages so it can handle WM_SYSCOMMAND
            // (This method is inlined)
            _systemMenu.HandleMessage(ref msg);
        }
        #endregion

        #region LoadFile
        private bool _loadFormOpened;
        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (_loadFormOpened) return;
            var flf = new FrmLoadFiles(this);
            flf.Load += (o, args) =>
            {
                _loadFormOpened = true;
                btnAnalyze.Enabled = false;
            };
            flf.Closed += (o, args) =>  {
                btnAnalyze.Enabled = FilePathsPair.Count > 0;
                _loadFormOpened = false;
            };
            flf.Show();
        }
        #endregion

        #region switch
        private void ChangeClipDisplay()
        {
            if (cbFileList.SelectedIndex < 0 || cbFileList.SelectedIndex > _fullData.Count) return;
            btnChart.Enabled = CurrentData.Data.Count > 0;
            UpdataGridView(CurrentData, FrameRate);
        }

        private void cbFileList_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ChangeClipDisplay();
            toolStripStatusStdError.Text = cbFileList.SelectedItem?.ToString();
        }

        private void cbFPS_SelectedIndexChanged(object sender, EventArgs e)
        {
            var frameRate = FrameRate;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Tag == null) continue;
                var temp = ToolKits.Second2Time((((int, double))row.Tag).Item1 / frameRate);
                row.Cells[2].Value = temp.Time2String();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            var threshold = Convert.ToInt32(numericUpDown1.Value);
            if (threshold == _threshold) return;
            if (_fullData == null || _fullData.Count == 0) return;
            UpdataGridView(CurrentData, FrameRate);
        }

        private void cbFileList_MouseEnter(object sender, EventArgs e) => toolTip1.Show(cbFileList.SelectedItem?.ToString(), (IWin32Window)sender);

        private void cbFileList_MouseLeave(object sender, EventArgs e) => toolTip1.RemoveAll();
        #endregion

        #region core
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
                numericUpDown1.Enabled = value;
                btnAbort.Enabled       = !value;
            }
        }
        private void UpdataGridView(ReSulT info, double frameRate)
        {
            dataGridView1.Rows.Clear();
            foreach (var item in info.Data)
            {
                if ((item.value > _threshold && dataGridView1.RowCount > 450) || dataGridView1.RowCount > 2048) break;
                var newRow = new DataGridViewRow {Tag = item};
                var temp = ToolKits.Second2Time(item.index / frameRate);
                newRow.CreateCells(dataGridView1, item.index, $"{item.value:F4}", temp.Time2String());
                newRow.DefaultCellStyle.BackColor = item.value < _threshold
                    ? Color.FromArgb(233, 76, 60) : Color.FromArgb(46, 205, 112);
                dataGridView1.Rows.Add(newRow);
                Application.DoEvents();
            }
            Debug.WriteLine($"DataGridView with {dataGridView1.Rows.Count} lines");
        }

        private bool _errorDialogShowed;
        private LogBuffer _currentBuffer;
        private List<(int index, double value)> _data;

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            _fullData.Clear();
            cbFileList.Items.Clear();
            foreach (var item in FilePathsPair)
            {
                try
                {
                    _errorDialogShowed = false;
                    _currentBuffer = new LogBuffer();
                    _data = new List<(int index, double value)>();

                    AnalyseClipLink(item);
                    var data = _data.OrderBy(a => a.value).ThenBy(a => a.index).ToList();
                    _fullData.Add(new ReSulT {FileNamePair = item, Data = data, Logs = _currentBuffer});
                    if (_currentBuffer.Inf) continue;
                    if (!(_coreProcess is VsPipePSNRProcess) || _remainFile || !UseOriginPath) continue;
                    RemoveScript(item);
                }
                catch (Exception ex)
                {
                    new Task(() => MessageBox.Show(
                                $"{item.src}{Environment.NewLine}{item.opt}{Environment.NewLine}{ex.Message}",
                                @"RPChecker ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error)).Start();
                }
            }
            _fullData.ForEach(item => cbFileList.Items.Add(Path.GetFileName(item.FileNamePair.src) ?? ""));
            if (cbFileList.Items.Count <= 0) return;
            cbFileList.SelectedIndex = 0;
            ChangeClipDisplay();
        }

        private bool _useOriginPath;

        private bool UseOriginPath => _useOriginPath || !(_coreProcess is VsPipePSNRProcess);

        private void AnalyseClipLink((string src, string opt) item)
        {
            Debug.Assert(item.src != null);
            Debug.Assert(item.opt != null);
            if (!UseOriginPath)
            {
                var linkedFile1 = Path.Combine(Path.GetPathRoot(item.src), Guid.NewGuid().ToString());
                var linkedFile2 = Path.Combine(Path.GetPathRoot(item.opt), Guid.NewGuid().ToString());
                NativeMethods.CreateHardLinkCMD(linkedFile1, item.src);
                NativeMethods.CreateHardLinkCMD(linkedFile2, item.opt);
                Debug.WriteLine($"HardLink: {item.src} => {linkedFile1}");
                Debug.WriteLine($"HardLink: {item.opt} => {linkedFile2}");
                AnalyseClip((linkedFile1, linkedFile2));
                File.Delete(linkedFile1);
                File.Delete(linkedFile2);
                RemoveScript((linkedFile1, linkedFile2));
            }
            else
            {
                AnalyseClip(item);
            }
        }

        private void AnalyseClip((string src, string opt) item)
        {
            _coreProcess.ProgressUpdated += ProgressUpdated;
            _coreProcess.ValueUpdated += ValueUpdated;

            Enable = false;
            toolStripStatusStdError.Text = _coreProcess.Loading;
            toolStripProgressBar1.Value = 0;
            try
            {
                Thread coreThread;
                if (_coreProcess is VsPipePSNRProcess)
                {
                    string vsFile = $"{item.opt}.vpy";
                    ToolKits.GenerateVpyFile(item, vsFile, (cbVpyFile.SelectedItem as FileInfo)?.FullName);
                    coreThread = new Thread(() => _coreProcess.GenerateLog(vsFile));
                }
                else
                {
                    coreThread = new Thread(() => _coreProcess.GenerateLog(item.src, item.opt));
                }
                coreThread.Start();
                while (coreThread.ThreadState != System.Threading.ThreadState.Stopped) Application.DoEvents();
                if (_coreProcess.Exceptions != null)
                {
                    toolStripStatusStdError.Text = _coreProcess.Exceptions.Message;
                    throw _coreProcess.Exceptions;
                }
            }
            catch (Exception ex)
            {
                new Task(() => MessageBox.Show(ex.Message, @"RPChecker Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)).Start();
            }
            finally
            {
                _coreProcess.ProgressUpdated -= ProgressUpdated;
                _coreProcess.ValueUpdated -= ValueUpdated;
                toolStripProgressBar1.Value = 100;
                Enable = true;
                Refresh();
                Application.DoEvents();
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            try
            {
                _coreProcess.Abort = true;
            }
            catch (Exception ex)
            {
                new Task(() => MessageBox.Show(ex.Message, "Terminate Process Failed")).Start();
            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            if (CurrentData.Logs.IsEmpty()) return;
            var log = new FormLog(CurrentData);
            log.Show();
            log.NormalizePosition();
        }

        private void ProgressUpdated(string progress)
        {
            if (string.IsNullOrEmpty(progress)) return;
            _currentBuffer.Log("err|" + progress);
            Invoke(new Action(() => toolStripStatusStdError.Text = progress));
            _coreProcess
                .Match<VsPipePSNRProcess>(_ => Invoke(new Action(() => VsUpdateProgress(progress))))
                .Match<FFmpegProcess>    (_ => Invoke(new Action(() => FFmpegUpdateProgress(progress))))
                ;
        }

        private void ValueUpdated(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            _currentBuffer.Log("std|" + data);
            _coreProcess
                .Match<VsPipePSNRProcess>(self => Invoke(new Action(() => self.UpdateValue(data, ref _data))))
                .Match<FFmpegProcess>    (self => Invoke(new Action(() => self.UpdateValue(data, ref _data))))
                ;
        }
        #endregion

        #region vapoursynth
        private static readonly Regex VsProgressRegex = new Regex(@"Frame: (?<processed>\d+)/(?<total>\d+)", RegexOptions.Compiled);

        private void VsUpdateProgress(string progress)
        {
            if (Regex.IsMatch(progress, "Failed|Error", RegexOptions.IgnoreCase))
            {
                _currentBuffer.Inf = true;
            }
            if (_currentBuffer.Inf)
            {
                if (!_errorDialogShowed && progress.Contains("No attribute with the name lsmas exists"))
                {
                    _errorDialogShowed = true;
                    new Task(() => MessageBox.Show(caption: @"RPChecker ERROR", icon: MessageBoxIcon.Error,
                        buttons: MessageBoxButtons.OK,
                        text: $"尚未安装 'L-SMASH' 滤镜{Environment.NewLine}大概的位置是在VapourSynth\\plugins64")).Start();
                }
                if (!_errorDialogShowed && progress.EndsWith("No module named 'mvsfunc'"))
                {
                    _errorDialogShowed = true;
                    new Task(() => MessageBox.Show(caption: @"RPChecker ERROR", icon: MessageBoxIcon.Error,
                        buttons: MessageBoxButtons.OK,
                        text: $"尚未正确放置mawen菊苣的滤镜库 'mvsfunc'{Environment.NewLine}大概的位置是在Python36\\Lib\\site-packages")).Start();
                }
                else if (!_errorDialogShowed && progress.EndsWith("There is no function named PlaneAverage"))
                {
                    _errorDialogShowed = true;
                    new Task(() => MessageBox.Show(caption: @"RPChecker ERROR", icon: MessageBoxIcon.Error,
                        buttons: MessageBoxButtons.OK,
                        text: $"请升级 'mvsfunc' 至少至 r6{Environment.NewLine}大概的位置是在Python36\\Lib\\site-packages")).Start();
                }
                return;
            }

            var ret = VsProgressRegex.Match(progress);
            if (!ret.Success) return;
            var processed = int.Parse(ret.Groups["processed"].Value);
            var total = int.Parse(ret.Groups["total"].Value);
            if (processed <= total)
            {
                toolStripProgressBar1.Value = (int)Math.Floor(processed * 100.0 / total);
            }
            Application.DoEvents();
        }
        #endregion

        #region ffmpeg
        private int _ffmpegTotalFrame = int.MaxValue;
        private static readonly Regex FFmpegFrameRegex = new Regex(@"NUMBER_OF_FRAMES: (?<frame>\d+)", RegexOptions.Compiled);
        private static readonly Regex FFmpegProgressRegex = new Regex(@"frame=\s*(?<processed>\d+)", RegexOptions.Compiled);
        private void FFmpegUpdateProgress(string progress)
        {
            // NUMBER_OF_FRAMES: 960
            //frame=  287 fps= 57 q=-0.0 size=N/A time=00:00:04.78 bitrate=N/A speed=0.953x
            if (progress.StartsWith("[Parsed_"))
            {
                _currentBuffer.Inf = true;
            }
            if (_currentBuffer.Inf) return;
            var frameRet = FFmpegFrameRegex.Match(progress);
            if (_ffmpegTotalFrame == int.MaxValue && frameRet.Success)
            {
                _ffmpegTotalFrame = int.Parse(frameRet.Groups["frame"].Value);
                return;
            }
            var ret = FFmpegProgressRegex.Match(progress);
            if (!ret.Success || _ffmpegTotalFrame == int.MaxValue) return;
            var processed = int.Parse(ret.Groups["processed"].Value);
            if (processed <= _ffmpegTotalFrame)
            {
                toolStripProgressBar1.Value = (int)Math.Floor(processed * 100.0 / _ffmpegTotalFrame);
            }
        }
        #endregion

        #region chartForm
        private bool _chartFormOpened;

        private void btnChart_Click(object sender, EventArgs e)
        {
            if (cbFileList.SelectedIndex < 0 || _chartFormOpened) return;
            var type = _coreProcess is VsPipePSNRProcess || _coreProcess is FFmpegPSNRProcess ? "PSNR" : "SSIM";
            var chart = new FrmChart(CurrentData, _threshold, FrameRate, type);
            chart.Load   += (o, args) => _chartFormOpened = true;
            chart.Closed += (o, args) => _chartFormOpened = false;
            chart.Show();
            chart.NormalizePosition();
        }
        #endregion

        #region cleanUpOption
        private static void RemoveScript((string src, string opt) item)
        {
            try
            {
                File.Delete($"{item.src}.lwi");
                File.Delete($"{item.opt}.lwi");
                File.Delete($"{item.opt}.vpy");
            }
            catch (Exception ex)
            {
                new Task(() => MessageBox.Show(ex.Message, @"RPChecker Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)).Start();
            }
        }

        private bool _remainFile;
        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {
            if (UseOriginPath)
            {
                _remainFile = !_remainFile;
                toolStripDropDownButton1.Image = _remainFile ? Resources.Checked : Resources.Unchecked;
            }
            else
            {
                Notification.ShowInfo("在硬链模式下该功能已被禁用", MessageBoxButtons.OK);
            }
        }

        private void toolStripDropDownButton1_MouseEnter(object sender, EventArgs e) => toolTip1.Show("保留中间文件", statusStrip1);

        private void toolStripDropDownButton1_MouseLeave(object sender, EventArgs e) => toolTip1.RemoveAll();
        #endregion

        #region about
        private readonly int[] _poi = { 0, 10 };

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {
            ++_poi[0];
            if (_poi[0] < 3 && _poi[1] == 10)
            {
                new Task(() => MessageBox.Show(@"Something happened", @"Something happened")).Start();
            }
            if (_poi[0] < _poi[1]) return;
            if (MessageBox.Show(@"是否打开关于界面", @"RPCheckerについて", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                new Form2().Show();
            }
            _poi[0]  = 00;
            _poi[1] += 10;
        }
        #endregion
    }

    public struct ReSulT
    {
        public List<(int index, double value)> Data { get; set; }
        public (string src, string opt) FileNamePair { get; set; }
        public LogBuffer Logs { get; set; }
    }
}

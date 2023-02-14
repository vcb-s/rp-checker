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
using Jil;

namespace RPChecker.Forms
{
    public partial class Form1 : Form
    {
        #region Form init

        private readonly IReadOnlyCollection<string> _rpcCollection;

        public Form1(IReadOnlyCollection<string> args)
        {
            InitializeComponent();
            AddCommand();

            _rpcCollection = args;
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

            Updater.Utils.CheckUpdateWeekly("RPChecker");
            if (_rpcCollection.Any())
            {
                LoadRPCFile(_rpcCollection);
            }

            var yThreshold = RegistryStorage.Load("threshold", "y");
            var uvThreshold = RegistryStorage.Load("threshold", "uv");
            if (yThreshold != null && int.TryParse(yThreshold, out int y))
            {
                _threshold = y;
                numericUpDown1.Value = y;
            }
            if (uvThreshold != null && int.TryParse(uvThreshold, out int uv))
            {
                _uv_threshold = uv;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _coreProcess.Kill();
            RegistryStorage.Save(Location.ToString(), @"Software\RPChecker", "Location");
        }
        #endregion

        public readonly List<(string src, string opt)> FilePathsPair = new List<(string src, string opt)>();
        private readonly List<ReSulT> _fullData = new List<ReSulT>();
        private int _threshold = 30;

        private int _uv_threshold = 40;
        private readonly double[] _frameRate = { 24000 / 1001.0, 24, 25, 30000 / 1001.0, 50, 60000 / 1001.0 };
        private IProcess _coreProcess = new VsPipePSNRProcess();

        private ReSulT CurrentData => _fullData[cbFileList.SelectedIndex];
        private double FrameRate   => _frameRate[cbFPS.SelectedIndex < 0 ? 0 : cbFPS.SelectedIndex];

        #region SystemMenu
        private SystemMenu _systemMenu;

        private void UpdateText(string value = null)
        {
            label1.Text = value ?? _coreProcess.ValueText;
            cbVpyFile.Enabled = _coreProcess is VsPipePSNRProcess;
            Text = $"[VCB-Studio] RP Checker v{Assembly.GetExecutingAssembly().GetName().Version} [{_coreProcess.Title}][{(UseOriginPath ? "O" : "L")}]";
            _threshold = _coreProcess.Threshold;
            numericUpDown1.Value = _threshold;
        }

        void SwitchPath()
        {
            _useOriginPath = !_useOriginPath;
            Text = Text.Substring(0, Text.Length - 3) + $"[{(UseOriginPath ? "O" : "L")}]";
        }

        void Set2VSPSNR()
        {
            _coreProcess = _coreProcess as VsPipePSNRProcess ?? new VsPipePSNRProcess();
            cbVpyFile.SelectedIndex = 0;
            UpdateText();
        }

        void Set2VSGMSD()
        {
            _coreProcess = _coreProcess as VsPipePSNRProcess ?? new VsPipePSNRProcess();
            cbVpyFile.SelectedIndex = 1;
            UpdateText("梯度幅度相似性");
            cbVpyFile_SelectedIndexChanged(cbVpyFile, null);
        }

        void Set2FFPSNR()
        {
            _coreProcess = _coreProcess as FFmpegPSNRProcess ?? new FFmpegPSNRProcess();
            cbVpyFile.SelectedIndex = 0;
            UpdateText();
        }

        void Set2FFSSIM()
        {
            _coreProcess = _coreProcess as FFmpegSSIMProcess ?? new FFmpegSSIMProcess();
            cbVpyFile.SelectedIndex = 0;
            UpdateText();
        }

        void LoadRPCFile(IEnumerable<string> rpcCollection)
        {
            _fullData.Clear();
            cbFileList.Items.Clear();
            try
            {
                foreach (var rpc in rpcCollection)
                {
                    var json = File.ReadAllText(rpc);
                    try
                    {
                        _fullData.AddRange(Jil.JSON.Deserialize<IEnumerable<ReSulT>>(json));
                    }
                    catch (Jil.DeserializationException)
                    {
                        try
                        {
                            _fullData.AddRange(Jil.JSON.Deserialize<IEnumerable<ResultV1>>(json).Select(x => (ReSulT)x));
                        }
                        catch (Jil.DeserializationException)
                        {
                            _fullData.AddRange(Jil.JSON.Deserialize<IEnumerable<ResultV2>>(json).Select(x => (ReSulT)x));
                        }
                    }
                }

                _fullData.ForEach(item =>
                {
                    cbFileList.Items.Add(Path.GetFileName(item.FileNamePair.src) ?? "");
                    item.Data.Sort(delegate((int, double, double, double) lhs, (int, double, double, double) rhs)
                    {
                        var lMin = Math.Min(lhs.Item2, Math.Min(lhs.Item3, lhs.Item4));
                        var rMin = Math.Min(rhs.Item2, Math.Min(rhs.Item3, rhs.Item4));
                        if (Math.Abs(lMin - rMin) > 1e-5)
                        {
                            return lMin - rMin < 0 ? -1 : 1;
                        }

                        return lhs.Item1 - rhs.Item1;
                    });
                });
                if (_fullData.Count > 0)
                {
                    cbFileList.SelectedIndex = 0;
                    ChangeClipDisplay();
                }
            }
            catch (Jil.DeserializationException ex)
            {
                MessageBox.Show($"JSON解析失败：\n{ex.Message}", @"RPChecker Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"载入失败：{ex.GetType()}\n{ex.Message}", @"RPChecker Error");
            }
            
        }

        private void AddCommand()
        {
            _systemMenu = new SystemMenu(this);
            _systemMenu.AddCommand("检查更新(&U)", () => { Updater.Utils.CheckUpdate(true); }, true);
            _systemMenu.AddCommand("使用 PSNR(VS)", Set2VSPSNR, true);
            _systemMenu.AddCommand("使用 GMSD(VS)", Set2VSGMSD, false);
            _systemMenu.AddCommand("使用 PSNR(FF)", Set2FFPSNR, false);
            _systemMenu.AddCommand("使用 SSIM(FF)", Set2FFSSIM, false);
            _systemMenu.AddCommand("使用原始路径", SwitchPath, true);
            _systemMenu.AddCommand("导出结果", () =>
            {
                try
                {
                    File.WriteAllText($"[RPCR] {DateTime.Now:yyyyMMddHHmmssffff}.rpc", Jil.JSON.Serialize(_fullData));
                }
                catch (Exception e)
                {
                    MessageBox.Show($"导出失败：\n{e.Message}", @"RPChecker Error");
                }
            }, true);
            _systemMenu.AddCommand("载入结果", () =>
            {
                var openFileDialog1 = new OpenFileDialog
                {
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    Filter = "RPC files (*.rpc)|*.rpc|Any files (*.*)|*.*",
                    FilterIndex = 0,
                    RestoreDirectory = true
                };

                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                LoadRPCFile(new []{ openFileDialog1.FileName });
            }, false);
            _systemMenu.AddCommand("重置路径", () =>
            {
                RegistryStorage.Save("");
                RegistryStorage.Save("", name: "FFmpegPath");
            }, true);
        }

        protected override void WndProc(ref Message msg)
        {
            base.WndProc(ref msg);

            // Let it know all messages so it can handle WM_SYSCOMMAND
            // (This method is inlined)
            _systemMenu.HandleMessage(ref msg);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Alt | Keys.NumPad1:
                case Keys.Alt | Keys.D1:
                    Set2VSPSNR();
                    return true;
                case Keys.Alt | Keys.NumPad2:
                case Keys.Alt | Keys.D2:
                    Set2VSGMSD();
                    return true;
                case Keys.Alt | Keys.NumPad3:
                case Keys.Alt | Keys.D3:
                    Set2FFPSNR();
                    return true;
                case Keys.Alt | Keys.NumPad4:
                case Keys.Alt | Keys.D4:
                    Set2FFSSIM();
                    return true;
                case Keys.Control | Keys.O:
                    btnLoad_Click(btnLoad, null);
                    return true;
                case Keys.Alt | Keys.Oem3:
                    SwitchPath();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
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
            UpdateGridView(CurrentData, FrameRate);
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
            _threshold = threshold;
            if (_fullData == null || _fullData.Count == 0) return;
            UpdateGridView(CurrentData, FrameRate);
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
        private void UpdateGridView(ReSulT info, double frameRate)
        {
            var timer = Stopwatch.StartNew();
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            dataGridView1.Rows.Clear();
            foreach (var item in info.Data)
            {
                var yWarningRequired = item.value_y < _threshold;
                var uWarningRequired = item.value_u < _uv_threshold;
                var vWarningRequired = item.value_v < _uv_threshold;
                var warningRequired = yWarningRequired || uWarningRequired || vWarningRequired;
                if ((!warningRequired && dataGridView1.RowCount > 1000) || dataGridView1.RowCount > 5000) break;
                var newRow = new DataGridViewRow {Tag = item};
                var time = ToolKits.Second2Time(item.index / frameRate);
                newRow.CreateCells(dataGridView1, item.index, Math.Round(item.value_y, 4), Math.Round(item.value_u, 4), Math.Round(item.value_v, 4), time.Time2String());

                newRow.Cells[1].Style.BackColor = yWarningRequired ? Color.FromArgb(233, 76, 60) : Color.FromArgb(46, 205, 112);
                newRow.Cells[2].Style.BackColor = uWarningRequired ? Color.FromArgb(233, 76, 60) : Color.FromArgb(46, 205, 112);
                newRow.Cells[3].Style.BackColor = vWarningRequired ? Color.FromArgb(233, 76, 60) : Color.FromArgb(46, 205, 112);

                dataGridView1.Rows.Add(newRow);
            }
            Application.DoEvents();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            Application.DoEvents();
            timer.Stop();
            Debug.WriteLine($"DataGridView with {dataGridView1.Rows.Count} lines in {timer.Elapsed.TotalSeconds}s");
        }

        private bool _errorDialogShowed;
        private LogBuffer _currentBuffer;
        private List<(int index, double value_y, double value_u, double value_v)> _data;

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
                    _data = new List<(int index, double value_y, double value_u, double value_v)>();

                    AnalyzeClipLink(item);
                    var data = _data.OrderBy(a => a.value_y).ThenBy(a => a.index).ToList();
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
            if (!IsHandleCreated || IsDisposed) return;
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            _fullData.ForEach(item => cbFileList.Items.Add(Path.GetFileName(item.FileNamePair.src) ?? ""));
            if (cbFileList.Items.Count <= 0) return;
            cbFileList.SelectedIndex = 0;
            ChangeClipDisplay();
            try
            {
                File.WriteAllText($"[RPCR] {DateTime.Now:yyyyMMddHHmmssffff}.rpc", Jil.JSON.Serialize(_fullData));
            }
            catch (Exception exception)
            {
                _currentBuffer.Log($"{exception.GetType()}: {exception.Message}");
            }
        }

        private bool _useOriginPath = true;

        private bool UseOriginPath => _useOriginPath || !(_coreProcess is VsPipePSNRProcess);

        private void AnalyzeClipLink((string src, string opt) item)
        {
            Debug.Assert(item.src != null);
            Debug.Assert(item.opt != null);
            if (!UseOriginPath)
            {
                var linkedFile1 = Path.Combine(Path.GetPathRoot(item.src), Guid.NewGuid() + Path.GetExtension(item.src));
                var linkedFile2 = Path.Combine(Path.GetPathRoot(item.opt), Guid.NewGuid() + Path.GetExtension(item.opt));
                var exitCode = 0;
                exitCode |= NativeMethods.CreateHardLinkCMD(linkedFile1, item.src);
                exitCode |= NativeMethods.CreateHardLinkCMD(linkedFile2, item.opt);
                if (exitCode != 0)
                {
                    _useOriginPath = true;
                    UpdateText();
                    AnalyzeClip(item);
                }
                else
                {
                    Debug.WriteLine($"HardLink: {item.src} => {linkedFile1}");
                    Debug.WriteLine($"HardLink: {item.opt} => {linkedFile2}");
                    AnalyzeClip((linkedFile1, linkedFile2));
                    File.Delete(linkedFile1);
                    File.Delete(linkedFile2);
                    RemoveScript((linkedFile1, linkedFile2));
                }
            }
            else
            {
                AnalyzeClip(item);
            }
        }

        private void AnalyzeClip((string src, string opt) item)
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
                    toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
                    var vsFile = $"{item.opt}.vpy";
                    ToolKits.GenerateVpyFile(item, vsFile,
                        cbVpyFile.SelectedItem is FileInfo info ? info.FullName : cbVpyFile.SelectedItem as string);
                    coreThread = new Thread(() => _coreProcess.GenerateLog(vsFile));
                }
                else
                {
                    toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
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
                new Task(() => MessageBox.Show($"校验过程中出现异常: \n{ex.Message}", @"RPChecker ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning)).Start();
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
            if (!IsHandleCreated || IsDisposed) return;

            _currentBuffer.Log("err|" + progress);
            Invoke(new Action(() => toolStripStatusStdError.Text = progress));
            _coreProcess
                .Match<VsPipePSNRProcess>(_ =>
                {
                    if (IsHandleCreated && !IsDisposed)
                        Invoke(new Action(() => VsUpdateProgress(progress)));
                })
                .Match<FFmpegProcess>(_ =>
                {
                    if (IsHandleCreated && !IsDisposed)
                        Invoke(new Action(() => FFmpegUpdateProgress(progress)));
                })
                ;
        }

        private void ValueUpdated(string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            _currentBuffer.Log("std|" + data);
            _coreProcess
                .Match<VsPipePSNRProcess>(self =>
                {
                    if (IsHandleCreated && !IsDisposed)
                        Invoke(new Action(() => self.UpdateValue(data, ref _data)));
                })
                .Match<FFmpegProcess>(self =>
                {
                    if (IsHandleCreated && !IsDisposed)
                        Invoke(new Action(() => self.UpdateValue(data, ref _data)));
                })
                ;
        }
        #endregion

        #region vapoursynth
        private static readonly Regex VsProgressRegex = new Regex(@"Frame: (?<processed>\d+)/(?<total>\d+)", RegexOptions.Compiled);
        private static readonly Regex VsErrorRegex = new Regex("Failed|Error", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private void VsUpdateProgress(string progress)
        {
            if (VsErrorRegex.IsMatch(progress))
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
                        text: $"尚未安装 'L-SMASH' 滤镜{Environment.NewLine}大概的位置是在VapourSynth\\plugins")).Start();
                }
                if (!_errorDialogShowed && progress.Contains("No attribute with the name complane exists"))
                {
                    _errorDialogShowed = true;
                    new Task(() => MessageBox.Show(caption: @"RPChecker ERROR", icon: MessageBoxIcon.Error,
                        buttons: MessageBoxButtons.OK,
                        text: $"尚未正确放置插件 'vs-ComparePlane' {Environment.NewLine}大概的位置是在VapourSynth\\plugins")).Start();
                }
                else if (!_errorDialogShowed && progress.EndsWith("There is no function named PlaneAverage"))
                {
                    _errorDialogShowed = true;
                    new Task(() => MessageBox.Show(caption: @"RPChecker ERROR", icon: MessageBoxIcon.Error,
                        buttons: MessageBoxButtons.OK,
                        text: $"请升级 'mvsfunc' 至少至 r6{Environment.NewLine}大概的位置是在Python3X\\Lib\\site-packages")).Start();
                }
                else if (!_errorDialogShowed && progress.EndsWith("ModuleNotFoundError: No module named 'muvsfunc'"))
                {
                    _errorDialogShowed = true;
                    new Task(() => MessageBox.Show(caption: @"RPChecker ERROR", icon: MessageBoxIcon.Error,
                        buttons: MessageBoxButtons.OK,
                        text: $"尚未正确放置滤镜库 'muvsfunc'{Environment.NewLine}大概的位置是在Python3X\\Lib\\site-packages")).Start();
                }
                return;
            }

            var ret = VsProgressRegex.Match(progress);
            if (!ret.Success) return;
            var processed = int.Parse(ret.Groups["processed"].Value);
            var total     = int.Parse(ret.Groups["total"].Value);
            var newProgressValue = (int)Math.Floor(processed * 100.0 / total);
            if (processed <= total && toolStripProgressBar1.Value != newProgressValue)
            {
                toolStripProgressBar1.Value = newProgressValue;
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

            if (_ffmpegTotalFrame == int.MaxValue)
            {
                var frameRet = FFmpegFrameRegex.Match(progress);
                if (frameRet.Success)
                {
                    _ffmpegTotalFrame = int.Parse(frameRet.Groups["frame"].Value);
                    toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
                }
                return;
            }
            var ret = FFmpegProgressRegex.Match(progress);
            if (!ret.Success || _ffmpegTotalFrame == int.MaxValue) return;
            var processed = int.Parse(ret.Groups["processed"].Value);
            var newProgressValue = (int)Math.Floor(processed * 100.0 / _ffmpegTotalFrame);
            if (processed <= _ffmpegTotalFrame && toolStripProgressBar1.Value != newProgressValue)
            {
                toolStripProgressBar1.Value = newProgressValue;
            }
        }
        #endregion

        #region chartForm
        private bool _chartFormOpened;

        private void btnChart_Click(object sender, EventArgs e)
        {
            if (cbFileList.SelectedIndex < 0 || _chartFormOpened) return;
            var type = _coreProcess.ValueText;
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
            var (src, opt) = item;
            try
            {
                File.Delete($"{src}.lwi");
                File.Delete($"{opt}.lwi");
                File.Delete($"{opt}.vpy");
            }
            catch (Exception ex)
            {
                new Task(() => MessageBox.Show($"删除临时文件时出现异常: \n{ex.Message}", @"RPChecker Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)).Start();
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
                using (var about = new Form2())
                {
                    about.Show();
                }
            }
            _poi[0]  = 00;
            _poi[1] += 10;
        }
        #endregion

        private void cbVpyFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            _threshold = ((ComboBox) sender).SelectedIndex == 1 ? 80 : 30;
            numericUpDown1.Value = _threshold;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            LoadRPCFile(fileList);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
    }


    public struct ReSulT
    {
        public List<(int index, double value_y, double value_u, double value_v)> Data { get; set; }
        public (string src, string opt) FileNamePair { get; set; }
        [JilDirective(Ignore = true)]
        public LogBuffer Logs { get; set; }

        public static implicit operator ReSulT(ResultV1 result)
        {
            return new ReSulT
            {
                Data = result.Data.Select(item => (item.index, item.value, 0.0, 0.0)).ToList(),
                FileNamePair = result.FileNamePair,
                Logs = result.Logs
            };
        }

        public static implicit operator ReSulT(ResultV2 result)
        {
            var data = new List<(int index, double value_y, double value_u, double value_v)>();
            foreach (var item in result.Data)
            {
                if (item.Count == 3)
                {
                    data.Add((item[0].index, item[0].value, item[1].value, item[2].value));
                }
                else
                {
                    data.Add((item[0].index, item[0].value, 0, 0));
                }
            }
            return new ReSulT
            {
                Data = data,
                FileNamePair = result.FileNamePair,
                Logs = result.Logs
            };
        }
    }

    public struct ResultV1
    {
        public List<(int index, double value)> Data { get; set; }
        public (string src, string opt) FileNamePair { get; set; }
        [JilDirective(Ignore = true)]
        public LogBuffer Logs { get; set; }
    }

    public struct ResultV2
    {
        public List<List<(int index, double value)>> Data { get; set; }
        public (string src, string opt) FileNamePair { get; set; }
        [JilDirective(Ignore = true)]
        public LogBuffer Logs { get; set; }
    }
}

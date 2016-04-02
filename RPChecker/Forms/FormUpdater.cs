using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace RPChecker.Forms
{
    public partial class FormUpdater : Form
    {
        private readonly string _newPath;

        private readonly string _exePath;

        private readonly string _backupPath;

        private WebClient _client;

        private readonly string _baseUrl;

        private readonly Version _version;

        public FormUpdater(string currentProgram, Version version, string baseUrl)
        {
            InitializeComponent();
            Icon              = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            _newPath          = currentProgram + ".new";
            _exePath          = currentProgram;
            _backupPath       = currentProgram + ".bak";
            labelVersion.Text = version.ToString();
            _baseUrl = baseUrl;
            _version = version;
        }

        private void FormUpdater_Load(object sender, EventArgs e)
        {
            _client = new WebClient();
            _client.DownloadFileCompleted   += client_DownloadFileCompleted;
            _client.DownloadProgressChanged += client_DownloadProgressChanged;
            _client.DownloadFileAsync(new Uri($"http://{_baseUrl}/bin/RPChecker.v{_version}.exe"), _newPath);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _client.CancelAsync();
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBarDownload.Value = e.ProgressPercentage;
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                if (File.Exists(_newPath))
                {
                    File.Delete(_newPath);
                }
                Close();
            }
            else
            {
                if (File.Exists(_backupPath))
                {
                    File.Delete(_backupPath);
                }
                File.Move(_exePath, _backupPath);
                File.Move(_newPath, _exePath);
                Application.Restart();
            }
        }
    }
}

using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RPChecker.Util
{
    public class FFmpegProcess: IProcess
    {
        private Process _consoleProcess;

        public bool Abort { get; set; }

        public int ExitCode { get; set; }

        public bool ProcessNotFind { get; set; }

        public string Loading => "读条中";

        public string FileNotFind => "无可用FFmpeg";

        public event Action<string> ProgressUpdated;

        public event Action<string> ValueUpdated;
        
        public void GenerateLog(object inputFilePair)
        {
            string ffmpegPath;
            try
            {
                ffmpegPath = RegistryStorage.Load(name : "FFmpegPath");
                if (!File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")))//the file has been moved
                {
                    ffmpegPath = Notification.InputBox("请输入FFmpeg的地址", "注意不要带上多余的引号", "C:\\FFmpeg\\ffmpeg.exe");
                    RegistryStorage.Save(Path.GetDirectoryName(ffmpegPath), name: "FFmpegPath");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ffmpegPath = string.Empty;
                if (!File.Exists("ffmpeg.exe"))
                {
                    ffmpegPath = Notification.InputBox("请输入FFmpeg的地址", "注意不要带上多余的引号", "C:\\FFmpeg\\ffmpeg.exe");
                    if (!File.Exists(ffmpegPath))
                    {
                        ProcessNotFind = true;
                        return;
                    }
                    ffmpegPath = Path.GetDirectoryName(ffmpegPath) ?? "";
                    RegistryStorage.Save(ffmpegPath, name: "FFmpegPath");
                }
            }
            ProcessNotFind = false;
            var inputFile = (KeyValuePair<string, string>)inputFilePair;
            try
            {
                _consoleProcess = new Process
                {
                    StartInfo =
                    {
                    FileName               = Path.Combine(ffmpegPath, "ffmpeg"),
                    Arguments              = $"-i \"{inputFile.Key}\" -i \"{inputFile.Value}\" -filter_complex ssim=\"stats_file=-\" -an -f null -",
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                    },
                    EnableRaisingEvents = true
                };

                _consoleProcess.OutputDataReceived += OutputHandler;
                _consoleProcess.ErrorDataReceived += ErrorOutputHandler;
                _consoleProcess.Exited += ExitedHandler;

                _consoleProcess.Start();
                _consoleProcess.BeginErrorReadLine();
                _consoleProcess.BeginOutputReadLine();
                _consoleProcess.WaitForExit();

                _consoleProcess.ErrorDataReceived -= ErrorOutputHandler;
                _consoleProcess.OutputDataReceived -= OutputHandler;
            }
            catch (Exception ex)
            {
                _consoleProcess = null;
                MessageBox.Show(ex.Message, @"FFmpeg Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            ValueUpdated?.Invoke(outLine.Data);
            //Debug.WriteLine("std: " + outLine.Data?.Trim());
            //format sample: n:946 Y:1.000000 U:0.999978 V:0.999984 All:0.999994 (51.994140)
        }

        public void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            ProgressUpdated?.Invoke(outLine.Data);
            //Debug.WriteLine("dbg: " + outLine.Data?.Trim());
            //format sample: frame=  287 fps= 57 q=-0.0 size=N/A time=00:00:04.78 bitrate=N/A speed=0.953x
            if (Abort)
            {
                ((Process)sendingProcess).Kill();
                Abort = false;
            }
        }

        public void ExitedHandler(object sender, EventArgs e)
        {
            ExitCode = _consoleProcess.ExitCode;
            Debug.WriteLine("Exit code: {0}", ExitCode);

            _consoleProcess.Close();
            _consoleProcess.Exited -= ExitedHandler;
        }
    }
}
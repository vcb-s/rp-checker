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

        public event Action<string> ProgressUpdated;

        public event Action<string> ValueUpdated;

        public void GenerateLog(object inputFileList)
        {
            string ffmpegPath;
            try
            {
                ffmpegPath = RegistryStorage.Load(name : "FFmpeg");
                if (!File.Exists(ffmpegPath + "FFmpeg.exe"))
                {
                    //ffmpegPath = ConvertMethod.GetVapourSynthPathViaRegistry();
                    //todo: show dialog to get the ffmpeg path
                    RegistryStorage.Save(ffmpegPath,name: "FFmpeg");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ffmpegPath = string.Empty;
                if (!File.Exists("FFmpeg.exe"))
                {
                    ProcessNotFind = true;
                    return;
                }
            }
            ProcessNotFind = false;
            var inputFile = inputFileList as List<string>;
            Debug.Assert(inputFile != null);
            try
            {
                _consoleProcess = new Process
                {
                    StartInfo =
                    {
                    FileName               = $"{ffmpegPath}FFmpeg",
                    Arguments              = $"-i \"{inputFile[0]}\" -i \"{inputFile[1]}\" -filter_complex ssim=\"stats_file=-\" -an -f null -",
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
            //format sample: n:946 Y:1.000000 U:0.999978 V:0.999984 All:0.999994 (51.994140)
        }

        public void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // No such file or directory
            ProgressUpdated?.Invoke(outLine.Data);
            Debug.WriteLine(outLine.Data);
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
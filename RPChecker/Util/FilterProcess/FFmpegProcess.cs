using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace RPChecker.Util.FilterProcess
{
    public abstract class FFmpegProcess: IProcess
    {
        private Process _consoleProcess;

        public bool Abort { get; set; }

        public int ExitCode { get; set; }

        public string Loading => "读条中";

        public string FileNotFind => "无可用FFmpeg";

        public event Action<string> ProgressUpdated;

        public event Action<string> ValueUpdated;

        public Exception Exceptions { get; set; }

        protected virtual string Arguments { get; } = null;

        public void GenerateLog(object inputFilePair)
        {
            string ffmpegPath = this.GetFFmpegPath(out Exception exception);
            if (exception != null || ffmpegPath == null)
            {
                Exceptions = exception;
                return;
            }
            var inputFile = (KeyValuePair<string, string>)inputFilePair;
            try
            {
                _consoleProcess = new Process
                {
                    StartInfo =
                    {
                    FileName               = Path.Combine(ffmpegPath, "ffmpeg"),
                    Arguments              = string.Format(Arguments, inputFile.Key, inputFile.Value),
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
                Exceptions = ex;
            }
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            ValueUpdated?.Invoke(outLine.Data);
        }

        public void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            ProgressUpdated?.Invoke(outLine.Data);
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

        protected const string Number = @"(?:[-+]?[0-9]*\.?[0-9]+)";

        public abstract void UpdateValue(string data, ref Dictionary<int, double> tempData);
    }
}

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

        public abstract string ValueText { get; }

        public virtual int Threshold => 30;

        public abstract string Title { get; }

        protected abstract string Arguments { get; }

        public void GenerateLog(params string[] inputFiles)
        {
            var ffmpegPath = this.GetFFmpegPath(out var exception);
            if (exception != null || ffmpegPath == null)
            {
                Exceptions = exception;
                return;
            }
            if (inputFiles.Length != 2)
            {
                Exceptions = new ArgumentException("Incorrect number of parameters", nameof(inputFiles));
                return;
            }
            try
            {
                _consoleProcess = new Process
                {
                    StartInfo =
                    {
                    FileName               = Path.Combine(ffmpegPath, "ffmpeg"),
                    Arguments              = string.Format(Arguments, inputFiles[0], inputFiles[1]),
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

        public void Kill()
        {
            try
            {
                _consoleProcess?.Kill();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected const string Number = @"(?:[-+]?[0-9]*\.?[0-9]+)";

        public abstract void UpdateValue(string data, ref List<(int index, double value_y, double value_u, double value_v)> tempData);
    }
}

﻿using System;
using System.Diagnostics;
using System.IO;

namespace RPChecker.Util.FilterProcess
{
    public class VsPipePSNRProcess: IProcess
    {
        private Process _consoleProcess;

        public bool Abort          { get; set; }

        public int ExitCode        { get; set; }

        public string Loading => "生成lwi文件中……";

        public string FileNotFind => "无可用vspipe";

        public event Action<string> ProgressUpdated;

        public event Action<string> ValueUpdated;

        public Exception Exceptions { get; set; }

        public string ValueText { get; } = "峰值信噪比阈值";

        public void GenerateLog(params string[] inputFiles)
        {
            string vspipePath = this.GetVsPipePath(out Exception exception);
            if (exception != null || vspipePath == null)
            {
                Exceptions = exception;
                return;
            }
            if (inputFiles.Length != 1)
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
                    FileName               = Path.Combine(vspipePath, "vspipe"),
                    Arguments              = $" -p \"{inputFiles[0]}\" .",
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                    },
                    EnableRaisingEvents    = true
                };

                _consoleProcess.OutputDataReceived += OutputHandler;
                _consoleProcess.ErrorDataReceived  += ErrorOutputHandler;
                _consoleProcess.Exited             += ExitedHandler;

                _consoleProcess.Start();
                _consoleProcess.BeginErrorReadLine();
                _consoleProcess.BeginOutputReadLine();
                _consoleProcess.WaitForExit();

                _consoleProcess.ErrorDataReceived  -= ErrorOutputHandler;
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
            //Debug.WriteLine(outLine.Data);
        }

        public void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            ProgressUpdated?.Invoke(outLine.Data);
            //Debug.WriteLine(outLine.Data);
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

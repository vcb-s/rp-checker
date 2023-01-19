using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

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

        public string ValueText => "峰值信噪比阈值";

        public int Threshold => 30;

        public string Title => "VapourSynth";

        private static readonly Regex PSNRDataFormatRegex = new Regex(@"(?<frame>\d+) (?<PSNR_Y>[-+]?[0-9]*\.?[0-9]+) (?<PSNR_U>[-+]?[0-9]*\.?[0-9]+) (?<PSNR_V>[-+]?[0-9]*\.?[0-9]+)", RegexOptions.Compiled);

        public void GenerateLog(params string[] inputFiles)
        {
            var vspipePath = this.GetVsPipePath(out var exception);
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

        public void UpdateValue(string data, ref List<(int index, double value_y, double value_u, double value_v)> tempData)
        {
            var rawData = PSNRDataFormatRegex.Match(data);
            if (!rawData.Success) return;
            tempData.Add(
                (
                int.Parse(rawData.Groups["frame"].Value),
                double.Parse(rawData.Groups["PSNR_Y"].Value),
                double.Parse(rawData.Groups["PSNR_U"].Value),
                double.Parse(rawData.Groups["PSNR_V"].Value)
                ));
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
    }
}

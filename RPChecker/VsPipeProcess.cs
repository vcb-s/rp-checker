using System;
using System.IO;
using System.Diagnostics;

namespace RPChecker
{
    public static class VsPipeProcess
    {
        private static Process _consoleProcess;

        public static bool Abort { private get; set; }

        private static int ExitCode { get; set; }

        public delegate void ProgressUpdatedEventHandler(string progress);

        public static  event ProgressUpdatedEventHandler ProgressUpdated;

        public delegate void PsnrDataUpdateEventHandler(string data);

        public static  event PsnrDataUpdateEventHandler PsnrUpdated;


        public static void GenerateLog(object scriptFile)
        {
            const bool value = true;
            string vspipePath = string.Empty;
            try
            {
                 vspipePath = ConvertMethod.GetVapourSynthPathViaRegistry();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (File.Exists("vspipe.exe"))
                {
                    throw new Exception("无有效的vspipe");
                }
            }
             _consoleProcess = new Process
            {
                StartInfo =
                {
                    FileName               = $"{vspipePath}vspipe",
                    Arguments              = $" -p \"{scriptFile}\" .",
                    UseShellExecute        = false,
                    CreateNoWindow         = value,
                    RedirectStandardOutput = value,
                    RedirectStandardError  = value
                },
                 EnableRaisingEvents       = true
            };

            _consoleProcess.OutputDataReceived += OutputHandler;
            _consoleProcess.ErrorDataReceived  += ErrorOutputHandler;
            _consoleProcess.Exited             += VsPipe_Exited;

            _consoleProcess.Start();
            _consoleProcess.BeginOutputReadLine();
            _consoleProcess.BeginErrorReadLine();
            _consoleProcess.WaitForExit();

            _consoleProcess.OutputDataReceived -= OutputHandler;
            _consoleProcess.ErrorDataReceived  -= ErrorOutputHandler;
        }


        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            PsnrUpdated?.Invoke(outLine.Data);
        }

        private static void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (Abort)
            {
                ((Process)sendingProcess).Kill();
                Abort = false;
                return;
            }
            Debug.WriteLine(outLine.Data);
            ProgressUpdated?.Invoke(outLine.Data);
        }

        private static void VsPipe_Exited(object sender, EventArgs e)
        {
             ExitCode = _consoleProcess.ExitCode;
             Debug.WriteLine("Exit code: {0}", ExitCode);
             _consoleProcess.Close();
             _consoleProcess.Dispose();
             _consoleProcess.Exited -= VsPipe_Exited;
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RPChecker.Util
{
    public static class VsPipeProcess
    {
        private static Process _consoleProcess;

        public static bool Abort         { private get; set; }

        private static int ExitCode      { get; set; }

        public static bool VsPipeNotFind { get; private set; }

        public static event Action<string> ProgressUpdated;

        public static event Action<string> ValueUpdated;


        public static void GenerateLog(object scriptFile)
        {
            
            string vspipePath;
            try
            {
                vspipePath = RegistryStorage.Load();
                if (!File.Exists(vspipePath + "vspipe.exe"))
                {
                    vspipePath = ToolKits.GetVapourSynthPathViaRegistry();
                    RegistryStorage.Save(vspipePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                vspipePath = string.Empty;
                if (!File.Exists("vspipe.exe"))
                {
                    VsPipeNotFind = true;
                    return;
                }
            }
            VsPipeNotFind = false;
            try
            {
                _consoleProcess = new Process
                {
                    StartInfo =
                    {
                    FileName               = $"{vspipePath}vspipe",
                    Arguments              = $" -p \"{scriptFile}\" .",
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
                MessageBox.Show(ex.Message, @"vspipe Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            ValueUpdated?.Invoke(outLine.Data);
            //Debug.WriteLine(outLine.Data);
        }

        private static void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            ProgressUpdated?.Invoke(outLine.Data);
            //Debug.WriteLine(outLine.Data);
            if (Abort)
            {
                ((Process)sendingProcess).Kill();
                Abort = false;
            }
        }

        private static void ExitedHandler(object sender, EventArgs e)
        {
            ExitCode = _consoleProcess.ExitCode;
            Debug.WriteLine("Exit code: {0}", ExitCode);

            _consoleProcess.Close();
            _consoleProcess.Exited -= ExitedHandler;
        }
    }
}

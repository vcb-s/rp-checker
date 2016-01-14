using System;
using System.Text;
using System.Diagnostics;

namespace RPChecker
{
    public static class VsPipeProcess
    {
        private static Process _consoleProcess;

        public static bool Abort { private get; set; }

        public static void GenerateLog(object scriptFile)
        {
            const bool value = true;
             _consoleProcess = new Process
            {
                StartInfo =
                {
                    FileName               = "vspipe",
                    Arguments              = $" -p \"{scriptFile}\" .",
                    UseShellExecute        = false,
                    CreateNoWindow         = value,
                    RedirectStandardOutput = value,
                    RedirectStandardError  = value
                }//,
                 //EnableRaisingEvents = true
            };

            _consoleProcess.OutputDataReceived += OutputHandler;
            _consoleProcess.ErrorDataReceived  += ErrorOutputHandler;
            //_consoleProcess.Exited             += VsPipe_Exited;

            _consoleProcess.Start();
            _consoleProcess.BeginOutputReadLine();
            _consoleProcess.BeginErrorReadLine();

            _consoleProcess.WaitForExit();

            Debug.WriteLine("Exit code: {0}", _consoleProcess.ExitCode);

            _consoleProcess.OutputDataReceived -= OutputHandler;
            _consoleProcess.ErrorDataReceived -= ErrorOutputHandler;

            // Check the exit code
            if (_consoleProcess.ExitCode > 0)
            {
                // something went wrong!
                //throw new Exception($"vspipe exited with error code {_consoleProcess.ExitCode}!");
            }
            if (_consoleProcess.ExitCode < 0)
            {
                // user aborted the current procedure!
                //throw new Exception("User aborted the current process!");
            }
        }

        public delegate void ProgressUpdatedEventHandler(string progress);

        public static event ProgressUpdatedEventHandler ProgressUpdated;

        public delegate void PsnrDataUpdateEventHandler(string data);

        public static event PsnrDataUpdateEventHandler PsnrUpdated;



        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //Console.WriteLine(outLine.Data);
            //StandOutput.Append(outLine.Data + Environment.NewLine);
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
            //MainForm.Invoke(new Action<string>(MainForm.UpdateError), CurrentErrorInfo);
        }

        private static void VsPipe_Exited(object sender, EventArgs e)
        {
            _consoleProcess.Close();
            _consoleProcess.Dispose();
        }

    }
}
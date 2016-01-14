using System;
using System.Diagnostics;
using System.Text;

namespace RPChecker
{
    public static class VsPipeProcess
    {
        private static Process _consoleProcess;

        public static void GenerateLog(object scriptFile)
        {
            bool value = true;

            _consoleProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "vspipe",
                    Arguments = $" -p \"{scriptFile}\" .",
                    UseShellExecute = false,
                    CreateNoWindow = value,
                    RedirectStandardOutput = value,
                    RedirectStandardError = value
                },
                EnableRaisingEvents = true
            };

            _consoleProcess.OutputDataReceived += OutputHandler;
            _consoleProcess.ErrorDataReceived += ErrorOutputHandler;
            _consoleProcess.Exited += VsPipe_Exited;
            //process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);

            _consoleProcess.Start();
            _consoleProcess.BeginOutputReadLine();
            _consoleProcess.BeginErrorReadLine();

            _consoleProcess.WaitForExit();
            //StandOutput = _consoleProcess.StandardOutput.ReadToEnd();
            //_consoleProcess.WaitForExit();
            //if (ExitCode == 0)
            //{
            //    _consoleProcess.Close();
            //}

            //return string.Empty; //StandOutput;
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
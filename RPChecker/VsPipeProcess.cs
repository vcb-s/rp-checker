using System;
using System.Diagnostics;
using System.Text;

namespace RPChecker
{
    public static class VsPipeProcess
    {
        public static Form1 mainForm;
        public static readonly StringBuilder StandOutput = new StringBuilder();
        private static Process consoleProcess;
        public static int exitCode = 0;
        public static string arguments;
        public static string currentErrorInfo;

        public volatile static bool _exited = false;
        public static void GenerateLog()
        {
            bool value = true;

            consoleProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "vspipe",
                    Arguments = $" -p \"{arguments}\" .",
                    UseShellExecute = false,
                    CreateNoWindow = value,
                    RedirectStandardOutput = value,
                    RedirectStandardError = value
                }
            };

            consoleProcess.EnableRaisingEvents = true;
            consoleProcess.OutputDataReceived += OutputHandler;
            consoleProcess.ErrorDataReceived += ErrorOutputHandler;
            consoleProcess.Exited += VsPipe_Exited;
            //process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);

            consoleProcess.Start();
            consoleProcess.BeginOutputReadLine();
            consoleProcess.BeginErrorReadLine();
            //StandOutput = consoleProcess.StandardOutput.ReadToEnd();
            //consoleProcess.WaitForExit();
            //if (exitCode == 0)
            //{
            //    consoleProcess.Close();
            //}


            //return string.Empty; //StandOutput;
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //Console.WriteLine(outLine.Data);
            StandOutput.Append(outLine.Data + Environment.NewLine);
        }

        private static void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Console.WriteLine(outLine.Data);
            currentErrorInfo = outLine.Data;
            mainForm.Invoke(new Action<string>(mainForm.UpdateError), currentErrorInfo);
        }

        private static void VsPipe_Exited(object sender, EventArgs e)
        {
            exitCode = consoleProcess.ExitCode;
            consoleProcess.Close();
            consoleProcess.Dispose();
            _exited = true;
        }
    }
}
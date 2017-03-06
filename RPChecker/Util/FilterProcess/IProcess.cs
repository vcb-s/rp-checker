using System;
using System.Diagnostics;

namespace RPChecker.Util.FilterProcess
{
    public interface IProcess
    {
        bool Abort { get; set; }

        int ExitCode { get; set; }

        void GenerateLog(params string[] inputFiles);

        void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine);

        void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine);

        void ExitedHandler(object sender, EventArgs e);
 
        event Action<string> ProgressUpdated;

        event Action<string> ValueUpdated;

        string Loading { get; }

        string FileNotFind { get; }

        string ValueText { get; }

        Exception Exceptions { get; set; }
    }
}
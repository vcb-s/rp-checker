using System;
using System.Diagnostics;

namespace RPChecker.Util
{
    public interface IProcess
    {
        bool Abort { get; set; }

        int ExitCode { get; set; }

        bool ProcessNotFind { get; set; }

        void GenerateLog(object args);

        void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine);

        void ErrorOutputHandler(object sendingProcess, DataReceivedEventArgs outLine);

        void Process_Exited(object sender, EventArgs e);
    }
}
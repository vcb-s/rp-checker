﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RPChecker.Util
{
    public class VsPipeProcess: IProcess
    {
        private Process _consoleProcess;

        public bool Abort          { get; set; }

        public int ExitCode        { get; set; }

        public bool ProcessNotFind { get; set; }

        public string Loading => "生成lwi文件中……";

        public string FileNotFind => "无可用vspipe";

        public event Action<string> ProgressUpdated;

        public event Action<string> ValueUpdated;


        public void GenerateLog(object scriptFile)
        {
            
            string vspipePath;
            try
            {
                vspipePath = RegistryStorage.Load();
                if (!File.Exists(Path.Combine(vspipePath, "vspipe.exe")))
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
                    ProcessNotFind = true;
                    return;
                }
            }
            ProcessNotFind = false;
            try
            {
                //ffmpeg -i file1.mkv -i fil2.mkv -filter_complex psnr="stats_file=-" -an -f null -
                _consoleProcess = new Process
                {
                    StartInfo =
                    {
                    FileName               = Path.Combine(vspipePath, "vspipe"),
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CommandLine;
using RPChecker.Forms;

namespace RPChecker
{
    class Options
    {
        [Option('r', "rpc", Required = false, HelpText = "Input rpc files to be processed.")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option(Default = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [Option("stdin", Default = false, HelpText = "Read from stdin")]
        public bool stdin { get; set; }
    }

    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;


        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // AttachConsole(ATTACH_PARENT_PROCESS);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => Application.Run(new Form1(opts.InputFiles.ToList())))
                .WithNotParsed(errs => MessageBox.Show(errs.First().ToString(), @"RPChecker Error"));
        }
    }
}

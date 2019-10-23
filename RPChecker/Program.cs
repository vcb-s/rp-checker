using System;
using System.Reflection;
using System.Windows.Forms;
using RPChecker.Forms;

namespace RPChecker
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Updater.Utils.SoftwareName = "RPChecker";
            Updater.Utils.RepoName = "vcb-s/rp-checker";
            Updater.Utils.CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Application.Run(new Form1());
        }
    }
}

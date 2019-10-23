using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace RPChecker.Util
{
    public class NativeMethods
    {
        [DllImport("shell32.dll", EntryPoint = "#680", CharSet = CharSet.Unicode)]
        public static extern bool IsUserAnAdmin();

        [DllImport("Kernel32.dll", EntryPoint = "CreateHardLinkW", CharSet = CharSet.Unicode)]
        private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        // https://docs.microsoft.com/en-us/windows/desktop/api/shlwapi/nf-shlwapi-pathfindonpathw
        // https://www.pinvoke.net/default.aspx/shlwapi.PathFindOnPath
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern bool PathFindOnPath([In, Out] StringBuilder pszFile, [In] string[] ppszOtherDirs);

        public static bool CreateHardLink(string lpFileName, string lpExistingFileName)
        {
            return CreateHardLink(lpFileName, lpExistingFileName, IntPtr.Zero);
        }

        public static void CreateHardLinkCMD(string lpFileName, string lpExistingFileName)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "fsutil",
                    Arguments = $"hardlink create \"{lpFileName}\" \"{lpExistingFileName}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}
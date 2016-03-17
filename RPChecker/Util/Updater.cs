using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RPChecker.Forms;

namespace RPChecker.Util
{
    public static class Updater
    {
        private static void OnResponse(IAsyncResult ar)
        {
            Regex versionRegex = new Regex(@"RPChecker (\d+\.\d+\.\d+\.\d+)");
            WebRequest webRequest = (WebRequest)ar.AsyncState;
            Stream responseStream;
            try
            {
                responseStream = webRequest.EndGetResponse(ar).GetResponseStream();
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format("检查更新失败, 错误信息:{0}{1}{0}请联系TC以了解详情",
                    Environment.NewLine, exception.Message), @"Update Check Fail");
                responseStream = null;
            }
            if (responseStream == null) return;

            StreamReader streamReader = new StreamReader(responseStream);
            string context = streamReader.ReadToEnd();
            var result = versionRegex.Match(context);
            if (!result.Success) return;

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Version remoteVersion = Version.Parse(result.Groups[1].Value);
            if (currentVersion >= remoteVersion)
            {
                MessageBox.Show($"v{currentVersion} 已是最新版", @"As Expected");
                return;
            }
            var dialogResult = MessageBox.Show(caption: @"Wow! Such Impressive", text: $"新车已发车 v{remoteVersion}，上车!",
                                               buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Asterisk);
            if (dialogResult != DialogResult.Yes) return;
            FormUpdater formUpdater = new FormUpdater(Application.ExecutablePath, remoteVersion);
            formUpdater.ShowDialog();
        }

        public static void CheckUpdate()
        {
            if (!IsConnectInternet()) return;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://tcupdate.applinzi.com/index.php");
            webRequest.UserAgent      = $"{Environment.UserName}({Environment.OSVersion}) / {Assembly.GetExecutingAssembly().GetName().FullName}";
            webRequest.Credentials    = CredentialCache.DefaultCredentials;
            webRequest.BeginGetResponse(OnResponse, webRequest);
        }

        public static bool CheckUpdateWeekly(string program)
        {
            var reg = RegistryStorage.Load(@"Software\" + program, "LastCheck");
            if (string.IsNullOrEmpty(reg))
            {
                RegistryStorage.Save(DateTime.Now.ToString(CultureInfo.InvariantCulture), @"Software\" + program, "LastCheck");
                return false;
            }
            var lastCheckTime = DateTime.Parse(reg);
            if (DateTime.Now - lastCheckTime > new TimeSpan(7, 0, 0, 0))
            {
                CheckUpdate();
                RegistryStorage.Save(DateTime.Now.ToString(CultureInfo.InvariantCulture), @"Software\" + program, "LastCheck");
                return true;
            }
            return false;
        }

        private static bool IsConnectInternet()
        {
            return InternetGetConnectedState(0, 0);
        }

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(int description, int reservedValue);
    }
}
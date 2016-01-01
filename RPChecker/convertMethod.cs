using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace RPChecker
{
    internal static class ConvertMethod
    {
        private static string GetUTF8String(byte[] buffer)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 3) return Encoding.UTF8.GetString(buffer);
            byte[] bomBuffer = { 0xef, 0xbb, 0xbf };

            if (buffer[0] == bomBuffer[0]
             && buffer[1] == bomBuffer[1]
             && buffer[2] == bomBuffer[2])
            {
                return new UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
            }
            return Encoding.UTF8.GetString(buffer);
        }

        public static TimeSpan Second2Time(double second)
        {
            double secondPart = Math.Floor(second);
            double millisecondPart = Math.Round((second - secondPart) * 1000);
            return new TimeSpan(0, 0, 0, (int)secondPart, (int)millisecondPart);
        }

        public static string Time2String(TimeSpan temp)
        {
            return temp.Hours.ToString("00") + ":" +
                 temp.Minutes.ToString("00") + ":" +
                 temp.Seconds.ToString("00") + "." +
            temp.Milliseconds.ToString("000");
        }


        private static void GenerateVpyFile(string file1, string file2, string outputFile, string selectedFile)
        {
            string template = "import sys\r\nimport vapoursynth as vs \r\nimport mvsfunc as mvf\r\nimport functools\r\ncore = vs.get_core(accept_lowercase = True)\r\ncore.max_cache_size = 5000\r\nsrc = core.lsmas.LWLibavSource(r\"%File1%\", threads = 1, format = \"yuv420p16\")\r\nopt = core.lsmas.LWLibavSource(r\"%File2%\", threads = 1, format = \"yuv420p16\")\r\ncmp = mvf.PlaneCompare(opt, src, mae = False, rmse = False, cov = False, corr = False)\r\ndef callback(n, clip, f):\r\n    print(n, f.props.PlanePSNR)\r\n    return clip\r\ncmp = core.std.FrameEval(cmp, functools.partial(callback, clip = cmp), prop_src =[cmp])\r\ncmp.set_output()\r\n";
            if (selectedFile != "Default")
            {
                var btemp = File.ReadAllBytes(selectedFile);
                string temp = GetUTF8String(btemp);
                if (temp.IndexOf("%File1%", StringComparison.Ordinal) > 0 &&
                    temp.IndexOf("%File2%", StringComparison.Ordinal) > 0 &&
                    temp.IndexOf("%LogFile%", StringComparison.Ordinal) > 0)
                {
                    template = temp;
                }
                else
                {
                    throw new ArgumentException("无效的模板文件");
                }
            }
            template = Regex.Replace(template, "%File1%", file1);
            template = Regex.Replace(template, "%File2%", file2);
            File.WriteAllText(outputFile, template, Encoding.Default);
        }

        private static int Compare(KeyValuePair<int, double> a, KeyValuePair<int, double> b)
        {
            return a.Value.CompareTo(b.Value);
        }

        public static List<KeyValuePair<int, double>> AnalyseFile(string file1, string file2, string vsFile, string selectedFile)
        {
            GenerateVpyFile(file1, file2, vsFile, selectedFile);
            var rawData = Regex.Matches(GenerateLog(vsFile), @"(?<fram>\d+) (?<PSNR>[-+]?[0-9]*\.?[0-9]+)");
            var data = (from Match line in rawData select new KeyValuePair<int, double>(int.Parse(line.Groups["fram"].Value), double.Parse(line.Groups["PSNR"].Value))).ToList();
            data.Sort(Compare);
            return data;
        }

        private static string GenerateLog(string arguments, bool value = true)
        {
            string output;
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "vspipe.exe";
                process.StartInfo.Arguments = $" -p \"{arguments}\" .";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = !value;
                process.StartInfo.RedirectStandardOutput = value;

                process.Start();
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                process.Close();
            }
            return output;
        }
    }
}

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace RPChecker.Util
{
    internal static class ConvertMethod
    {
        private static string GetUTF8String(byte[] buffer)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 3) return Encoding.UTF8.GetString(buffer);

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
            {
                return new UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
            }
            return Encoding.UTF8.GetString(buffer);
        }

        private static readonly Regex RTimeFormat = new Regex(@"(?<Hour>\d+):(?<Minute>\d+):(?<Second>\d+)\.(?<Millisecond>\d{3})");

        public static TimeSpan Second2Time(double second)
        {
            double secondPart      = Math.Floor(second);
            double millisecondPart = Math.Round((second - secondPart) * 1000);
            return new TimeSpan(0, 0, 0, (int)secondPart, (int)millisecondPart);
        }

        public static string Time2String(this TimeSpan temp) => $"{temp.Hours:D2}:{temp.Minutes:D2}:{temp.Seconds:D2}.{temp.Milliseconds:D3}";

        public static TimeSpan ToTimeSpan(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return TimeSpan.Zero; }
            var temp = RTimeFormat.Match(input);
            int hour = int.Parse(temp.Groups["Hour"].Value);
            int minute = int.Parse(temp.Groups["Minute"].Value);
            int second = int.Parse(temp.Groups["Second"].Value);
            int millisecond = int.Parse(temp.Groups["Millisecond"].Value);
            return new TimeSpan(0, hour, minute, second, millisecond);
        }

        public static void GenerateVpyFile(string file1, string file2, string outputFile, string selectedFile)
        {
            //"import sys\r\nimport vapoursynth as vs \r\nimport mvsfunc as mvf\r\nimport functools\r\ncore = vs.get_core(accept_lowercase = True)\r\ncore.max_cache_size = 5000\r\nsrc = core.lsmas.LWLibavSource(r\"%File1%\", format = \"yuv420p16\")\r\nopt = core.lsmas.LWLibavSource(r\"%File2%\", format = \"yuv420p16\")\r\ncmp = mvf.PlaneCompare(opt, src, mae = False, rmse = False, cov = False, corr = False)\r\ndef callback(n, clip, f):\r\n    print(n, f.props.PlanePSNR)\r\n    return clip\r\ncmp = core.std.FrameEval(cmp, functools.partial(callback, clip = cmp), prop_src =[cmp])\r\ncmp.set_output()\r\n";
            string template = Properties.Resources.vpyTemplate;
            if (selectedFile != "Default")
            {
                string temp = GetUTF8String(File.ReadAllBytes(selectedFile));
                if (!temp.Contains(@"%File1%") || !temp.Contains(@"%File2%"))
                {
                    throw new FormatException("无效的模板文件");
                }
                template = temp;
            }
            if (Path.GetDirectoryName(file1) == Path.GetDirectoryName(file2))
            {
                file1 = Path.GetFileName(file1);
                file2 = Path.GetFileName(file2);
            }
            template = template.Replace(@"%File1%", file1);
            template = template.Replace(@"%File2%", file2);
            File.WriteAllText(outputFile, template, Encoding.UTF8);
        }

        public static Point String2Point(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new Point(-32000, -32000);
            var rpos = new Regex(@"{X=(?<x>.+),Y=(?<y>.+)}");
            var result = rpos.Match(input);
            if (!result.Success) return new Point(-32000, -32000);
            int x = int.Parse(result.Groups["x"].Value);
            int y = int.Parse(result.Groups["y"].Value);
            return new Point(x, y);
        }

        /// <summary>
        /// Returns the path from VapourSynth.
        /// It tries to find it via the registry keys.
        /// If it doesn't find it, it throws an exception.
        /// </summary>
        /// <returns></returns>
        public static string GetVapourSynthPathViaRegistry()
        {
            RegistryKey regVapourSynth = null;
            string valuePath           = string.Empty;
            bool subKeyFound           = false;
            bool valueFound            = false;
            // First check Win32 registry
            using (RegistryKey regUninstall32 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                if (regUninstall32 == null) throw new Exception("Failed to create a RegistryKey variable");
                if (regUninstall32.GetSubKeyNames().Any(subKeyName => subKeyName.ToLower().Equals("VapourSynth_is1".ToLower())))
                {
                    subKeyFound = true;
                    regVapourSynth = regUninstall32.OpenSubKey("VapourSynth_is1");
                    Debug.Assert(regVapourSynth != null);
                }
                // if sub key was found, try to get the executable path
                if (subKeyFound)
                {
                    foreach (string valueName in regVapourSynth.GetValueNames().Where(valueName => valueName.ToLower().Equals("InstallLocation".ToLower())))
                    {
                        valueFound = true;
                        valuePath = (string)regVapourSynth.GetValue(valueName);
                        break;
                    }
                }
            }
            // if value was not found, let's Win64 registry
            if (!valueFound)
            {
                subKeyFound = false;
                using (RegistryKey regUninstall64 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"))
                {
                    if (regUninstall64 == null) throw new Exception("Failed to create a RegistryKey variable");
                    if (regUninstall64.GetSubKeyNames().Any(subKeyName => subKeyName.ToLower().Equals("VapourSynth_is1".ToLower())))
                    {
                        subKeyFound = true;
                        regVapourSynth = regUninstall64.OpenSubKey("VapourSynth_is1");
                        Debug.Assert(regVapourSynth != null);
                    }
                    // if sub key was found, try to get the executable path
                    if (subKeyFound)
                    {
                        foreach (string valueName in regVapourSynth.GetValueNames().Where(valueName => valueName.ToLower().Equals("InstallLocation".ToLower())))
                        {
                            valuePath = (string)regVapourSynth.GetValue(valueName);
                            break;
                        }
                    }
                    if (!subKeyFound) throw new Exception("Found VapourSynth in your system but not the registry value InstallLocation!");
                }
            }
            valuePath = valuePath ?? "";
            if (!File.Exists(valuePath+@"core64\vspipe.exe"))
            {
                throw new Exception($"Found a registry value ({valuePath}) for VapourSynth in your system but it is not valid!");
            }
            return valuePath + @"core64\";
        }
    }
}

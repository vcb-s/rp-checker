using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace RPChecker.Util
{
    internal static class ToolKits
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
            var secondPart      = Math.Floor(second);
            var millisecondPart = Math.Round((second - secondPart) * 1000);
            return new TimeSpan(0, 0, 0, (int)secondPart, (int)millisecondPart);
        }

        public static string Time2String(this TimeSpan temp) => $"{temp.Hours:D2}:{temp.Minutes:D2}:{temp.Seconds:D2}.{temp.Milliseconds:D3}";

        public static TimeSpan ToTimeSpan(this string input)
        {
            if (string.IsNullOrEmpty(input)) { return TimeSpan.Zero; }
            var temp        = RTimeFormat.Match(input);
            var hour        = int.Parse(temp.Groups["Hour"].Value);
            var minute      = int.Parse(temp.Groups["Minute"].Value);
            var second      = int.Parse(temp.Groups["Second"].Value);
            var millisecond = int.Parse(temp.Groups["Millisecond"].Value);
            return new TimeSpan(0, hour, minute, second, millisecond);
        }

        public static void GenerateVpyFile((string src, string opt) item, string outputFile, string selectedFile)
        {
            var template = Properties.Resources.vpyTemplate;
            if (selectedFile != "Default")
            {
                var temp = GetUTF8String(File.ReadAllBytes(selectedFile));
                if (!temp.Contains(@"%File1%") || !temp.Contains(@"%File2%"))
                {
                    throw new FormatException("无效的模板文件");
                }
                template = temp;
            }
            if (Path.GetDirectoryName(item.src) == Path.GetDirectoryName(item.opt))
            {
                item.src = Path.GetFileName(item.src);
                item.opt = Path.GetFileName(item.opt);
            }
            template = template.Replace(@"%File1%", item.src);
            template = template.Replace(@"%File2%", item.opt);
            File.WriteAllText(outputFile, template, Encoding.UTF8);
        }

        public static Point String2Point(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new Point(-32000, -32000);
            var rpos = new Regex(@"{X=(?<x>.+),Y=(?<y>.+)}");
            var result = rpos.Match(input);
            if (!result.Success) return new Point(-32000, -32000);
            var x = int.Parse(result.Groups["x"].Value);
            var y = int.Parse(result.Groups["y"].Value);
            return new Point(x, y);
        }

        public static void NormalizePosition(this System.Windows.Forms.Form form)
        {
            if (form.Location.X + form.Width < 0 ||
                form.Location.Y + form.Height < 0)
                form.Location = Point.Empty;
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
            var valuePath              = string.Empty;
            var subKeyFound            = false;
            var valueFound             = false;
            // First check Win32 registry
            using (var regUninstall32 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
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
                    foreach (var valueName in regVapourSynth.GetValueNames().Where(valueName => valueName.ToLower().Equals("InstallLocation".ToLower())))
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
                using (var regUninstall64 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"))
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
                        foreach (var valueName in regVapourSynth.GetValueNames().Where(valueName => valueName.ToLower().Equals("InstallLocation".ToLower())))
                        {
                            valuePath = (string)regVapourSynth.GetValue(valueName);
                            break;
                        }
                    }
                    if (!subKeyFound) throw new Exception("Can not found VapourSynth in your system!");
                }
            }
            valuePath = valuePath ?? "";
            if (!File.Exists(Path.Combine(valuePath, "core64\\vspipe.exe")))
            {
                throw new Exception($"Found a registry value ({valuePath}) for VapourSynth in your system but it is not valid!");
            }
            return Path.Combine(valuePath, "core64");
        }
    }
}

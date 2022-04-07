using System;
using System.Collections.Generic;
using System.IO;
using Jil;
using Microsoft.Win32;

namespace RPChecker.Util
{
    public static class RegistryStorage
    {
        private static readonly string configFile = "rp-checker.json";
        private static readonly string workDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static readonly string configPath = Path.Combine(workDir, configFile);

        public static string Load(string subKey = @"Software\RPChecker", string name = "VapourSynthPath")
        {
            if (!File.Exists(configPath))
            {
                return null;
            }
            var configText = File.ReadAllText(configPath);
            try
            {
                var config = JSON.Deserialize<Dictionary<string, string>>(configText);
                config.TryGetValue($"{subKey}.{name}", out string value);
                return value;
            }
            catch (DeserializationException)
            {
                File.Delete(configPath);
            }
            return null;
        }

        public static void Save(string value, string subKey = @"Software\RPChecker", string name = "VapourSynthPath")
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            if(File.Exists(configPath))
            {
                try
                {
                    var configText = File.ReadAllText(Path.Combine(workDir, configFile));
                    config = JSON.Deserialize<Dictionary<string, string>>(configText);
                }
                catch (DeserializationException)
                {
                    File.Delete(configPath);
                }
            }
            config[$"{subKey}.{name}"] = value;
            File.WriteAllText(Path.Combine(workDir, configFile), JSON.Serialize(config, new Jil.Options(true)));
        }

        public static void RegistryAddCount(string subKey, string name, int delta = 1)
        {
            var countS = Load(subKey, name);
            var count = string.IsNullOrEmpty(countS) ? 0 : int.Parse(countS);
            count += delta;
            Save(count.ToString(), subKey, name);
        }

        public static void RegistryAddTime(string subKey, string name, TimeSpan time)
        {
            var currentTime = Load(subKey, name).ToTimeSpan() + time;
            Save(currentTime.Time2String(), subKey, name);
        }
    }
}
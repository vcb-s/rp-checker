using System;
using Microsoft.Win32;

namespace RPChecker.Util
{
    public static class RegistryStorage
    {
        public static string Load(string subKey = @"Software\RPChecker", string name = "VapourSynthPath")
        {
            var path = string.Empty;
            // HKCU_CURRENT_USER\Software\
            var registryKey = Registry.CurrentUser.OpenSubKey(subKey);
            if (registryKey == null) return path;
            path = (string)registryKey.GetValue(name);
            registryKey.Close();
            return path;
        }

        public static void Save(string value, string subKey = @"Software\RPChecker", string name = "VapourSynthPath")
        {
            // HKCU_CURRENT_USER\Software\
            var registryKey = Registry.CurrentUser.CreateSubKey(subKey);
            registryKey?.SetValue(name, value);
            registryKey?.Close();
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
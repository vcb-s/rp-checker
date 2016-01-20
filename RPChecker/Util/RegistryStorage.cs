using Microsoft.Win32;

namespace RPChecker.Util
{
    public static class RegistryStorage
    {
        public static string Load(string subKey = @"Software\RPChecker", string name = "VapourSynthPath")
        {
            string path = string.Empty;
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

        public static void RegistryAddCount(string subKey, string name)
        {
            var countS = Load(subKey, name);
            int count = string.IsNullOrEmpty(countS) ? 0 : int.Parse(countS);
            Save($"{++count}", subKey, name);
        }
    }
}
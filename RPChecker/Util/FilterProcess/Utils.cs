using System;
using System.Diagnostics;
using System.IO;

namespace RPChecker.Util.FilterProcess
{
    public static class Utils
    {
        public static string GetVsPipePath(this IProcess process, out Exception exception)
        {
            exception = null;
            string vspipePath;
            try
            {
                vspipePath = RegistryStorage.Load();
                if (vspipePath == null || !File.Exists(Path.Combine(vspipePath, "vspipe.exe")))
                {
                    vspipePath = ToolKits.GetVapourSynthPathViaRegistry();
                    RegistryStorage.Save(vspipePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                vspipePath = string.Empty;
                if (!File.Exists("vspipe.exe"))
                {
                    exception = ex;
                    return null;
                }
            }
            return vspipePath;
        }


        public static string GetFFmpegPath(this IProcess process, out Exception exception)
        {
            exception = null;
            string ffmpegPath;
            try
            {
                ffmpegPath = RegistryStorage.Load(name: "FFmpegPath") ?? "";
                if (!File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")))//the file has been moved
                {
                    ffmpegPath = Notification.InputBox("请输入FFmpeg的地址", "注意不要带上多余的引号", "C:\\FFmpeg\\ffmpeg.exe");
                    RegistryStorage.Save(Path.GetDirectoryName(ffmpegPath), name: "FFmpegPath");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ffmpegPath = string.Empty;
                if (!File.Exists("ffmpeg.exe"))
                {
                    ffmpegPath = Notification.InputBox("请输入FFmpeg的地址", "注意不要带上多余的引号", "C:\\FFmpeg\\ffmpeg.exe");
                    if (!File.Exists(ffmpegPath))
                    {
                        exception = new Exception(process.FileNotFind);
                        return null;
                    }
                    ffmpegPath = Path.GetDirectoryName(ffmpegPath) ?? "";
                    RegistryStorage.Save(ffmpegPath, name: "FFmpegPath");
                }
            }
            return ffmpegPath;
        }
    }
}
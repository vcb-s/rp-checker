using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RPChecker.Util.FilterProcess
{
    public class FFmpegPSNRProcess: FFmpegProcess
    {
        public override string ValueText => "峰值信噪比阈值";

        public override string Title => "FFmpeg PSNR";

        protected override string Arguments => "-i \"{0}\" -i \"{1}\" -hide_banner -filter_complex psnr=\"stats_file=-\" -an -f null -";

        private static readonly string NumberOrInf = $"{Number}|inf";

        private static readonly Regex PSNRDataFormatRegex = new Regex($@"n:(?<frame>\d+) .*? psnr_avg:(?<avg>{NumberOrInf}) psnr_y:(?<y>{NumberOrInf}) psnr_u:(?<u>{NumberOrInf}) psnr_v:(?<v>{NumberOrInf})", RegexOptions.Compiled);

        public override void UpdateValue(string data, ref List<(int index, double value_y, double value_u, double value_v)> tempData)
        {
            //format sample: n:950 mse_avg:0.00 mse_y:0.00 mse_u:0.01 mse_v:0.00 psnr_avg:88.29 psnr_y:inf psnr_u:82.77 psnr_v:84.45
            var rawData = PSNRDataFormatRegex.Match(data);
            if (!rawData.Success) return;
            var psnr = rawData.Groups["avg"].Value;
            var psnrValue = TryParse(psnr);
            tempData.Add((int.Parse(rawData.Groups["frame"].Value), psnrValue, 0, 0));
        }

        private static double TryParse(string s, double defaultValue = 100)
        {
            return double.TryParse(s, out var result) ? result : defaultValue;
        }
    }
}

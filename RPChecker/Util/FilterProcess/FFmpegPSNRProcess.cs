using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RPChecker.Util.FilterProcess
{
    public class FFmpegPSNRProcess: FFmpegProcess
    {
        public override string ValueText { get; } = "峰值信噪比阈值";

        protected override string Arguments { get; } = "-i \"{0}\" -i \"{1}\" -hide_banner -filter_complex psnr=\"stats_file=-\" -an -f null -";

        private static readonly Regex PSNRDataFormatRegex = new Regex($@"n:(?<frame>\d+) .*? psnr_avg:(?<avg>{Number}) psnr_y:(?<y>{Number}) psnr_u:(?<u>{Number}) psnr_v:(?<v>{Number})", RegexOptions.Compiled);

        public override void UpdateValue(string data, ref Dictionary<int, double> tempData)
        {
            //format sample: n:950 mse_avg:0.00 mse_y:0.00 mse_u:0.01 mse_v:0.00 psnr_avg:88.29 psnr_y:inf psnr_u:82.77 psnr_v:84.45
            var rawData = PSNRDataFormatRegex.Match(data);
            if (!rawData.Success) return;
            string psnr = rawData.Groups["avg"].Value;
            double psnrValue = double.Parse(psnr);
            tempData[int.Parse(rawData.Groups["frame"].Value)] = psnrValue;
        }
    }
}

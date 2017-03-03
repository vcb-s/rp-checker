using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RPChecker.Util.FilterProcess
{
    public class FFmpegSSIMProcess: FFmpegProcess
    {
        public override string ValueText { get; } = "结构相似性阈值";

        protected override string Arguments { get; } = "-i \"{0}\" -i \"{1}\" -filter_complex ssim=\"stats_file=-\" -an -f null -";

        private static readonly Regex SSIMDataFormatRegex = new Regex($@"n:(?<frame>\d+) Y:(?<Y>{Number}) U:(?<U>{Number}) V:(?<V>{Number}) All:(?<All>{Number})", RegexOptions.Compiled);

        public override void UpdateValue(string data, ref Dictionary<int, double> tempData)
        {
            //format sample: n:946 Y:1.000000 U:0.999978 V:0.999984 All:0.999994 (51.994140|inf)
            var rawData = SSIMDataFormatRegex.Match(data);
            if (!rawData.Success) return;
            string ssim = rawData.Groups["All"].Value;
            double ssimValue = double.Parse(ssim) * 100;
            tempData[int.Parse(rawData.Groups["frame"].Value)] = ssimValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPChecker
{
    static class convertMethod
    {
        public static string GetUTF8String(byte[] buffer)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 3) return Encoding.UTF8.GetString(buffer);
            byte[] bomBuffer = new byte[] { 0xef, 0xbb, 0xbf };

            if (buffer[0] == bomBuffer[0]
             && buffer[1] == bomBuffer[1]
             && buffer[2] == bomBuffer[2])
            {
                return new UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
            }
            return Encoding.UTF8.GetString(buffer);
        }

        public static TimeSpan second2Time(double second)
        {
            double secondPart = Math.Floor(second);
            double millisecondPart = Math.Round((second - secondPart) * 1000);
            return new TimeSpan(0, 0, 0, (int)secondPart, (int)millisecondPart);
        }

        public static string time2string(TimeSpan temp)
        {
            return temp.Hours.ToString("00") + ":" +
                 temp.Minutes.ToString("00") + ":" +
                 temp.Seconds.ToString("00") + "." +
            temp.Milliseconds.ToString("000");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TeamworkMsprojectExportTweaker
{
    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/bb968652(v=office.12).aspx
    /// A duration of time, provided in the format PnYnMnDTnHnMnS where nY represents the number of years,
    /// nM the number of months, nD the number of days, T the date/time separator,
    /// nH the number of hours, nM the number of minutes, and nS the number of seconds.
    ///
    /// In Project only PT160H20M30S used.
    /// </summary>
    public static class TimeParser
    {
        private const string FormatTemplate = "PT{0}H{1}M{2}S";

        public static TimeSpan ToTime(string duration)
        {
            var parts = duration.SplitByStringFormat(FormatTemplate)
                .Select(x => int.Parse(x))
                .ToList();
            return new TimeSpan(parts[0], parts[1], parts[2]);
        }

        public static string ToString(TimeSpan duration)
        {
            var hours = (int)Math.Floor(duration.TotalHours);
            return string.Format(FormatTemplate, hours, duration.Minutes, duration.Seconds);
        }

        public static IList<string> SplitByStringFormat(this string str, string template)
        {
            // replace all the special regex characters
            template = Regex.Replace(template, @"[\\\^\$\.\|\?\*\+\(\)]", x => "\\" + x.Value);
            string pattern = "^" + Regex.Replace(template, @"\{[0-9]+\}", "(.*?)") + "$";

            Regex r = new Regex(pattern);
            Match m = r.Match(str);

            List<string> ret = new List<string>();

            for (int i = 1; i < m.Groups.Count; i++)
            {
                ret.Add(m.Groups[i].Value);
            }

            return ret;
        }
    }
}
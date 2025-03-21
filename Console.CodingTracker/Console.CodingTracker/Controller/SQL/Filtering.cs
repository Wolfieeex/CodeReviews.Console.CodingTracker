﻿using Console.CodingTracker.Model;

namespace Console.CodingTracker.Controller.SQL;
internal class Filtering
{
    internal static string AddQueryFilterParameters(FilterDetails filter, string whereInject, Dictionary<string, object> parameters)
    {
        if (filter != null)
        {

            if (!string.IsNullOrEmpty(filter.FromDate))
            {
                whereInject += $@"AND @FromDate <=  substr(""End date"", 7, 4) || '-' || substr(""End date"", 4, 2) || '-' || substr(""End date"", 1, 2) || ' ' || substr(""End date"", 13, 5) || ':00 ' ";
                parameters.Add("@FromDate", DateTimeSqliteStringConvert(filter.FromDate));
            }
            if (!string.IsNullOrEmpty(filter.ToDate))
            {
                whereInject += $@"AND @ToDate >= substr(""End date"", 7, 4) || '-' || substr(""End date"", 4, 2) || '-' || substr(""End date"", 1, 2) || ' ' || substr(""End date"", 13, 5) || ':00 ' ";
                parameters.Add("@ToDate", DateTimeSqliteStringConvert(filter.ToDate));
            }
            if (!string.IsNullOrEmpty(filter.MinDuration))
            {
                whereInject += $@"AND @MinDuration <= IIF(instr(Duration, '.') == 0, '', substr(Duration, 0, instr(Duration, '.'))) * 24 * 3600 + 3600 * substr(Duration, instr(Duration, ':') - 2, 2) + 60 * substr(Duration, instr(Duration, ':') + 1, 2) + substr(Duration, instr(Duration, ':') + 4, 2) ";
                parameters.Add("@MinDuration", TimeSpanSqliteStringConvert(filter.MinDuration));
            }
            if (!string.IsNullOrEmpty(filter.MaxDuration))
            {
                whereInject += $@"AND @MaxDuration >= IIF(instr(Duration, '.') == 0, '', substr(Duration, 0, instr(Duration, '.'))) * 24 * 3600 + 3600 * substr(Duration, instr(Duration, ':') - 2, 2) + 60 * substr(Duration, instr(Duration, ':') + 1, 2) + substr(Duration, instr(Duration, ':') + 4, 2) ";
                parameters.Add("@MaxDuration", TimeSpanSqliteStringConvert(filter.MaxDuration));
            }
            if (!string.IsNullOrEmpty(filter.MinLines))
            {
                whereInject += $@"AND ""Lines of code"" >= @MinLines ";
                parameters.Add("@MinLines", filter.MinLines);
            }
            if (!string.IsNullOrEmpty(filter.MaxLines))
            {
                whereInject += $@"AND ""Lines of code"" <= @MaxLines ";
                parameters.Add("@MaxLines", filter.MaxLines);
            }
            if (!string.IsNullOrEmpty(filter.Comment))
            {
                whereInject += $@"AND Comments LIKE lower(@Comment) ";
                parameters.Add("@Comment", "%" + filter.Comment.ToLower() + "%");
            }
            if (!string.IsNullOrEmpty(filter.WasTimerTracked))
            {
                whereInject += $@"AND ""Was Timer Tracked"" = @WasTimerTracked";
                parameters.Add("@WasTimerTracked", filter.WasTimerTracked == "True" ? 1 : 0);
            }

            if (whereInject.Contains("AND"))
            {
                whereInject = whereInject.Substring(4, whereInject.Length - 4);
                whereInject = whereInject.Insert(0, "WHERE ");
            }
        }

        return whereInject;
    }
    internal static string DateTimeSqliteStringConvert(string datetime)
    {
        string returnDate = "";
        returnDate = datetime.Substring(6, 4) + "-" + datetime.Substring(3, 2) + "-" + datetime.Substring(0, 2) + " ";
        returnDate += datetime.Substring(12, 2) + ":" + datetime.Substring(15, 2) + ":" + "00";
        return returnDate;
    }
    internal static int TimeSpanSqliteStringConvert(string timespan)
    {
        int timespanCalculated = 0;
        int spacePosition = timespan.IndexOf(' ');
        int ColonPosition = timespan.IndexOf(':');
        timespanCalculated += spacePosition == -1 ? 0 : int.Parse(timespan.Substring(0, spacePosition)) * 24 * 3600;
        timespanCalculated += int.Parse(timespan.Substring(ColonPosition - 2, 2)) * 3600;
        timespanCalculated += int.Parse(timespan.Substring(ColonPosition + 1, 2)) * 60;

        return timespanCalculated;
    }
}

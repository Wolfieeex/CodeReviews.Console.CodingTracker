using Console.CodingTracker.Model;
using Spectre.Console;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.View;

enum ReportTableTitles
{
    RecordCount,
    RecordSum,
    MaximalValue,
    MinimalValue,
    Average,
    Median,
    Mode
}

internal class Tables
{
    public static void DrawDatatable(List<CodingSession> list, bool[] viewSettings, bool automaticalDataFormatting = true)
    {

        Type type = list[0].GetType();

        Table table = new Table();


        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        string[] names = Array.ConvertAll(fields, f => f.Name);
        names = names.Skip(1).ToArray();
        for (int i = 0; i < names.Length; i++)
        {
            names[i] = Regex.Match(names[i], @"(?<=<)[A-Za-z0-9]*(?=>)").Value;
            names[i] = Regex.Replace(names[i], @"(?<=.+)([A-Z])", @" $1");
            names[i] = names[i].Insert(0, "[yellow]");
            names[i] += "[/]";
        }

        int interval = 0;
        table.AddColumn("[yellow]Index[/]");
        foreach (string n in names)
        {
            if (viewSettings.Length > interval)
            {
                if (viewSettings[interval] == true)
                {
                    table.AddColumn(n);
                }
            }
            else
            {
                table.AddColumn(n);
            }
            interval++;
        }

        interval = 0;
        foreach (CodingSession line in list)
        {
            interval++;
            List<string> newRow = new();
            newRow.Add(interval.ToString());
            for (int i = 1; i < fields.Length; i++)
            {
                if (viewSettings[i - 1] == true)
                {
                    string rowValue = fields[i].GetValue(line) == null ? "" : fields[i].GetValue(line).ToString();
                    if (automaticalDataFormatting)
                    {
                        bool isMatch = Regex.IsMatch(rowValue, @"(?<=^1)\.+(?=\d{2}:\d{2}:\d{2}$)");
                        if (isMatch)
                        {
                            rowValue = Regex.Replace(rowValue, @"(?<=^1)\.+(?=\d{2}:\d{2}:\d{2}$)", @" day, ");
                        }
                        else
                        {
                            rowValue = Regex.Replace(rowValue, @"(?<=^[0-9]*)\.+(?=\d{2}:\d{2}:\d{2}$)", @" days, ");
                        }
                        rowValue = rowValue == "-1" ? "-" : rowValue;
                    }
                    newRow.Add(rowValue);
                }
            }
            string[] rowToAdd = newRow.ToArray();
            table.AddRow(rowToAdd);
        }
        table.Title("View previous records:", new Style().Foreground(Color.LightPink3));
        table.Expand();
        table.Border = TableBorder.DoubleEdge;
        table.ShowRowSeparators();
        table.BorderColor(Color.SteelBlue3);
        AnsiConsole.Write(table);
    }

    public static void DrawReportTable(ReportSettings settings, Dictionary<string, List<string>> DurationTable, Dictionary<string, List<string>> LinesTable)
    {
        Type type = typeof(ReportTableTitles);

        Table table = new Table();

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        string[] names = Array.ConvertAll(fields, f => f.Name);
        names = names.Skip(1).ToArray();
        for (int i = 0; i < names.Length; i++)
        {
            names[i] = Regex.Match(names[i], @"(?<=<)[A-Za-z0-9]*(?=>)").Value;
            names[i] = Regex.Replace(names[i], @"(?<=.+)([A-Z])", @" $1");
            names[i] = names[i].Insert(0, "[yellow]");
            names[i] += "[/]";
        }

        int recordsLength = 0;
        int lineLength = 0;
        if (DurationTable != null)
        {
            recordsLength = DurationTable.Count;
            lineLength = DurationTable.ElementAt(0).Value.Count;
        }
        else
        {
            recordsLength = LinesTable.Count;
            lineLength = LinesTable.ElementAt(0).Value.Count;
        }

        int interval = 0;
        table.AddColumn("[yellow]Index[/]");
        foreach (string n in names)
        {
            if (settings.ReportOptions.Length > interval)
            {
                if (settings.ReportOptions[interval] == true)
                {
                    table.AddColumn(n);
                }
            }
            else
            {
                table.AddColumn(n);
            }
            interval++;
        }

        int line = 0;
        for (int i = 0; i < recordsLength; i++)
        {
            line++;
            List<string> lines = new List<string>();
            string linePeriod = "";
            linePeriod += line.ToString() + ") ";

            for (int j = 0; j < lineLength; j++)
            {
                string cell = "";
                if (settings.DataOptions[0] == true)
                {
                    AddPeriod(ref cell, settings, DurationTable.ElementAt(j).Key);
                    lines.Add(cell);

                    DurationTable
                }
                if (settings.DataOptions[1] == true)
                {
                    if (cell != "")
                    {
                        cell += "/n";
                    }
                    else
                    {
                        AddPeriod(ref cell, settings, LinesTable.ElementAt(j).Key);
                        lines.Add(cell);
                    }
                }
            }
            
        }
        table.Title("Report printed:", new Style().Foreground(Color.DeepPink1));
        table.Expand();
        table.Border = TableBorder.SimpleHeavy;
        table.ShowRowSeparators();
        table.BorderColor(Color.BlueViolet);
        AnsiConsole.Write(table);
    }

    private static void AddPeriod(ref string cell, ReportSettings settings, string facePeriod)
    {
        if (!String.IsNullOrEmpty(settings.SortationYear.ToString()))
        {
            cell += settings.SortationYear.ToString();

            if (settings.SortationMonth.ToString() != null)
            {
                cell += ", " + Enum.GetName(typeof(Months), settings.SortationMonth);
                cell += ", " + facePeriod;
            }
            else
            {
                cell += ", " + facePeriod;
            }
        }
        else
        {
            cell += facePeriod;
        }

        
    }
}

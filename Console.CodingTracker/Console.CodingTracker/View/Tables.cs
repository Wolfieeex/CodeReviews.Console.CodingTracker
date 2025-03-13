using Console.CodingTracker.Model;
using Spectre.Console;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Console.CodingTracker.View;

enum ReportTableTitles
{
    RecordsCount,
    RecordsSum,
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

        foreach (var col in table.Columns)
        {
            col.Width(col.Width + 3);
            col.RightAligned().Padding(1, 0);
            col.NoWrap();
        }
        table.Columns[0].Width(table.Columns[0].Width + 3).LeftAligned().Padding(1, 0);

        table.Centered();

        table.Title("View previous records:", new Style().Foreground(Color.LightPink3));
        table.Border = TableBorder.DoubleEdge;
        table.ShowRowSeparators();
        table.BorderColor(Color.SteelBlue3);
        AnsiConsole.Write(table);
    }
    public static void DrawReportTable(ReportSettings settings, Dictionary<string, List<string>> DurationTable, Dictionary<string, List<string>> LinesTable)
    {
        Table table = new Table();

        string[] names = Enum.GetNames(typeof(ReportTableTitles));
        for (int i = 0; i < names.Length; i++)
        {
            names[i] = Regex.Replace(names[i], @"(?<=.+)([A-Z])", @" $1");
            names[i] = names[i].Insert(0, "[yellow]");
            names[i] += ":[/]";
        }

        int recordsLength = 0;
        int lineLength = 0;
        if (DurationTable.ElementAt(0).Value.Count != 0)
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
        table.AddColumn("[yellow]Index:[/]");
        table.AddColumn("[yellow]Data type:[/]");
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

            string data = "";
            if (settings.DataOptions[0] == true)
            {
                AddPeriod(ref linePeriod, settings, DurationTable.ElementAt(i).Key);
                data += "[blue]Time span -->[/]";
                if (settings.DataOptions[1] == true)
                {
                    data += "\n[green]Lines number -->[/]";
                }
            }
            else
            {
                AddPeriod(ref linePeriod, settings, LinesTable.ElementAt(i).Key);
                data += "[green]Lines number -->[/]";
            }
            lines.Add(linePeriod);
            lines.Add(data);

            for (int j = 0; j < lineLength; j++)
            {
                int padding = 5;
                string cell = "";
                if (settings.DataOptions[0] == true)
                {
                    cell = DurationTable.ElementAt(i).Value[j];
                    if (j != 0 || settings.ReportOptions[0] == false)
                    {
                        cell = cell.Insert(0, "[blue]");
                        cell += "[/]";
                    }
                    if (settings.DataOptions[1] == true)
                    {
                        if (LinesTable.ElementAt(i).Value[j] == null || LinesTable.ElementAt(i).Value[j] == "N/A")
                        {
                            cell += "\nN/A";
                        }
                        else
                        {
                            cell += "\n" + String.Format("{0:0.##}", decimal.Parse(LinesTable.ElementAt(i).Value[j]));
                        }
                        if (j != 0 || settings.ReportOptions[0] == false)
                        {
                            cell = cell.Insert(0, "[green]");
                            cell += "[/]";
                        }
                    }
                }
                else
                {
                    if (LinesTable.ElementAt(i).Value[j] == null || LinesTable.ElementAt(i).Value[j] == "N/A")
                    {
                        cell += "\nN/A";
                    }
                    else
                    {
                        cell += "\n" + String.Format("{0:0.##}", decimal.Parse(LinesTable.ElementAt(i).Value[j]));
                    }
                    if (j != 0 || settings.ReportOptions[0] == false)
                    {
                        cell = cell.Insert(0, "[green]");
                        cell += "[/]";
                    }
                }
                lines.Add(cell);
            }
            table.AddRow(lines.ToArray());
        }

        foreach (var col in table.Columns)
        {
            col.Width(col.Width + 3);
            col.RightAligned().Padding(1, 0);
            col.NoWrap();
        }
        table.Columns[0].Width(table.Columns[0].Width + 3).LeftAligned().Padding(1, 0);

        table.Centered();
        table.Title("Report generation:", new Style().Foreground(Color.DeepPink1));
        table.Border = TableBorder.Rounded;
        table.ShowRowSeparators();
        table.BorderColor(Color.BlueViolet);
        System.Console.Clear();
        AnsiConsole.Write(table);
        string finishingText = "Report printed. Press any button to return to the report menu: ";
        System.Console.SetCursorPosition((System.Console.WindowWidth - finishingText.Length) / 2, System.Console.CursorTop);
        AnsiConsole.Markup("Report printed. [green]Press any button to return to the report menu: [/]");
        System.Console.ReadKey();
        System.Console.Clear();
    }
    private static void AddPeriod(ref string cell, ReportSettings settings, string facePeriod)
    {
        switch (settings.Period)
        {
            case ReportSortationPeriod.Yearly:
                cell += facePeriod;
                break;
            case ReportSortationPeriod.Monthly:
                cell += settings.SortationYear.ToString();
                cell += ", " + (Months)(int.Parse(facePeriod) - 1);
                break;
            case ReportSortationPeriod.Weekly:
                cell += settings.SortationYear.ToString();
                cell += ", week " + facePeriod.Substring(0, facePeriod.IndexOf('.'));
                break;
            case ReportSortationPeriod.Daily:
                cell += settings.SortationYear.ToString();
                cell += ", " + Enum.GetName(typeof(Months), settings.SortationMonth);
                cell += ", day " + facePeriod;
                break;
        }
    }
}

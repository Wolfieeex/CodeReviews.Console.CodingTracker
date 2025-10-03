using CodingTracker.Wolfieeex.Model;
using Spectre.Console;
using static CodingTracker.Wolfieeex.Model.ReportingEnums;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CodingTracker.Wolfieeex.View;

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
    public static void DrawReportTable(ReportSettings settings, Dictionary<string, List<string>> DurationTable, Dictionary<string, List<string>> LinesTable, string titleHex, string inputHex)
    {
        Table table = new Table();
        StringBuilder csvTable = new StringBuilder();

        string[] names = Enum.GetNames(typeof(ReportTableTitles));
		string[] csvNames = Enum.GetNames(typeof(ReportTableTitles));
		for (int i = 0; i < names.Length; i++)
        {
            names[i] = Regex.Replace(names[i], @"(?<=.+)([A-Z])", @" $1");
            csvNames[i] = Regex.Replace(csvNames[i], @"(?<=.+)([A-Z])", @" $1");

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
        csvTable.Append("Index:;");
		csvTable.Append("Data type:;");
		table.AddColumn("[yellow]Index:[/]");
        table.AddColumn("[yellow]Data type:[/]");
        for (int i = 0; i < names.Length; i++)
        {
            if (settings.ReportOptions.InfoToBoolArray().Length > interval)
            {
                if (settings.ReportOptions.InfoToBoolArray()[interval] == true)
                {
                    csvTable.Append(csvNames[i] + ":;");
                    table.AddColumn(names[i]);
                }
            }
            else
            {
				csvTable.Append(csvNames[i] + ":;");
				table.AddColumn(names[i]);
            }
            interval++;
        }

		int line = 0;
        for (int i = 0; i < recordsLength; i++)
        {
			csvTable.AppendLine();

			line++;
            List<string> lines = new List<string>();
            string linePeriod = "";
            string csvLinePeriod = "";
            linePeriod += line.ToString() + ") ";

            string data = "";
            string csvData = "\"";
            if (settings.DataOptions.InfoToBoolArray()[0] == true)
            {
                AddPeriod(ref linePeriod, settings, DurationTable.ElementAt(i).Key);
                csvLinePeriod = linePeriod + ";";

				data += "[blue]Time span -->[/]";
                csvData += "Time span";

				if (settings.DataOptions.InfoToBoolArray()[1] == true)
                {
                    data += "\n[green]Lines number -->[/]";
					csvData += "\nLines number";
				}

                csvData += "\";";
            }
            else
            {
                AddPeriod(ref linePeriod, settings, LinesTable.ElementAt(i).Key);
				csvLinePeriod = linePeriod + ";";

				data += "[green]Lines number -->[/]";
                csvData += "Lines number;";

			}

            lines.Add(linePeriod);
            lines.Add(data);

            csvTable.Append(csvLinePeriod);
            csvTable.Append(csvData);

			for (int j = 0; j < lineLength; j++)
            {
                string cell = "";
                string csvCell = "";

                if (settings.DataOptions.InfoToBoolArray()[0] == true)
                {
                    csvCell += "\"";
					cell = DurationTable.ElementAt(i).Value[j];
					csvCell += DurationTable.ElementAt(i).Value[j];

					if (j != 0 || settings.ReportOptions.InfoToBoolArray()[0] == false)
                    {
                        cell = cell.Insert(0, "[blue]");
                        cell += "[/]";
                    }
                    if (settings.DataOptions.InfoToBoolArray()[1] == true)
                    {
                        if (LinesTable.ElementAt(i).Value.Count == 0)
                        {
                            cell += "\nN/A";
							csvCell += "\nN/A";
						}
                        else
                        {
                            if (LinesTable.ElementAt(i).Value[j] == null || LinesTable.ElementAt(i).Value[j] == "N/A")
                            {
                                cell += "\nN/A";
								csvCell += "\nN/A";
							}
                            else
                            {
                                cell += "\n" + String.Format("{0:0.##}", decimal.Parse(LinesTable.ElementAt(i).Value[j]));
								csvCell += "\n" + String.Format("{0:0.##}", decimal.Parse(LinesTable.ElementAt(i).Value[j]));
							}
                            if (j != 0 || settings.ReportOptions.InfoToBoolArray()[0] == false)
                            {
                                cell = cell.Insert(0, "[green]");
                                cell += "[/]";
                            }
                        }
                    }
					csvCell += "\";";
				}
                else
                {
					csvCell += "\"";
					if (LinesTable.ElementAt(i).Value[j] == null || LinesTable.ElementAt(i).Value[j] == "N/A")
                    {
                        cell += "\nN/A";
						csvCell += "\nN/A";
					}
                    else
                    {
                        cell += "\n" + String.Format("{0:0.##}", decimal.Parse(LinesTable.ElementAt(i).Value[j]));
						csvCell += "\n" + String.Format("{0:0.##}", decimal.Parse(LinesTable.ElementAt(i).Value[j]));
					}
                    if (j != 0 || settings.ReportOptions.InfoToBoolArray()[0] == false)
                    {
                        cell = cell.Insert(0, "[green]");
                        cell += "[/]";
                    }
					csvCell += "\";";
				}
                lines.Add(cell);
                csvTable.Append(csvCell);
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
        AnsiConsole.Write(new Rule("-"));
		AnsiConsole.Write(new Markup($"Report printed. {titleHex}Insert \"s\" to save it[/] or {inputHex}any other text to return to the report menu[/]: "));
        string lineInput = System.Console.ReadLine();
		System.Console.Clear();

		if (lineInput.ToLower() == "s")
		{
            string pathVersion = "";
            int versionInterval = 1;

			string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @$"/Coding Tracker Report {DateTime.Now.ToString("dd.MM.yyyy HH-mm")}{pathVersion}.csv";
            while (File.Exists(desktopPath))
            {
                pathVersion = " v" + versionInterval;
                versionInterval++;
                desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @$"/Coding Tracker Report {DateTime.Now.ToString("dd.MM.yyyy HH-mm")}{pathVersion}.csv";
			}

			File.WriteAllText(desktopPath, csvTable.ToString());
			AnsiConsole.Write(new Markup($"Report {titleHex}saved to your desktop[/]. Press {inputHex}any key to return to the previous menu:[/] "));
			System.Console.ReadKey();
			System.Console.Clear();
		}
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

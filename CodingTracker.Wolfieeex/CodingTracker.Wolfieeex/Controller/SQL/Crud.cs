using CodingTracker.Wolfieeex.Model;
using Dapper;
using Microsoft.Data.Sqlite;
using static CodingTracker.Wolfieeex.Model.ReportingEnums;

namespace CodingTracker.Wolfieeex.Controller.SQL;

internal class Crud
{
    public static List<CodingSession> CurrentSessions { get; private set; } = new List<CodingSession>();
    internal static string InjectRecord(CodingSession session)
    {
        TimeSpan codingSpan = Helpers.CalculateDuration(session.StartDate, session.EndDate);
        string stringCodingSpan = codingSpan.ToString();

        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            string databaseName = System.Configuration.ConfigurationManager.AppSettings.Get("DatabaseName");

			string commString = @$"INSERT INTO '{databaseName}' 
                                ('Creation date', 'Last update date', 'Start date', 'End date', Duration, 'Lines of code', Comments, 'Was Timer Tracked')
                                VALUES (@Creation, @Update, @Start, @End, @Duration, @Lines, @Comments, @Timer)";
            var newRow = new { Creation = session.CreationDate, Update = session.LastUpdateDate, Start = session.StartDate, End = session.EndDate, session.Duration, Lines = session.NumberOfLines.HasValue ? session.NumberOfLines : -1, Comments = string.IsNullOrEmpty(session.Comments) ? "" : session.Comments, Timer = session.WasTimerTracked ? 1 : 0 };
            conn.Execute(commString, newRow);
            conn.Close();
        }
        return stringCodingSpan;
    }
    internal static void UpdateRecords(List<int> indexNumbers, string newData, MenuSelections.UpdateMenu updateSegment)
    {
        string columnUpdateName = "";
        switch (updateSegment)
        {
            case MenuSelections.UpdateMenu.UpdateStartDate:
                columnUpdateName = "Start date";
                break;
            case MenuSelections.UpdateMenu.UpdateEndDate:
                columnUpdateName = "End date";
                break;
            case MenuSelections.UpdateMenu.UpdateNumberOfLines:
                columnUpdateName = "Lines of code";
                break;
            case MenuSelections.UpdateMenu.UpdateComments:
                columnUpdateName = "Comments";
                break;
        }
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            string stringDateNow = DateTime.Now.ToString("dd/MM/yyyy, HH:mm");
            string updateDateCommand = $@"Update '{System.Configuration.ConfigurationManager.AppSettings.Get("DatabaseName")}' SET 'Last update date' = @stringDate WHERE Id = @id";
            string updateCommand = $@"Update '{System.Configuration.ConfigurationManager.AppSettings.Get("DatabaseName")}' SET '{columnUpdateName}' = @updateValue WHERE Id = @id";

            for (int i = 0; i < indexNumbers.Count; i++)
            {
                conn.Execute(updateDateCommand, new { stringDate = stringDateNow, id = indexNumbers[i] });
                conn.Execute(updateCommand, new { updateValue = newData, id = indexNumbers[i] });

                if (columnUpdateName == "Creation date" || columnUpdateName == "End date")
                {
                    string updateDurationCommand = $@"Update '{System.Configuration.ConfigurationManager.AppSettings.Get("DatabaseName")}' SET Duration = @duration WHERE Id = @id";
                    string retreiveDatesCommand = @$"SELECT ""Start date"", ""End date"" FROM {System.Configuration.ConfigurationManager.AppSettings.Get("DatabaseName")} WHERE Id = @id";
                    System.Data.IDataReader idr = conn.ExecuteReader(retreiveDatesCommand, new { id = indexNumbers[i] });
                    idr.Read();
                    string durationCalculated = Helpers.CalculateDuration(idr.GetString(0), idr.GetString(1)).ToString();
                    conn.Execute(updateDurationCommand, new { duration = durationCalculated, id = indexNumbers[i] });
                    idr.Close();
                }
            }
            conn.Close();
        }
    }
    internal static List<CodingSession> GetRecords(FilterDetails filter)
    {
        List<CodingSession> records = new List<CodingSession>();

        string whereInject = "";
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        whereInject = Filtering.AddQueryFilterParameters(filter, whereInject, parameters);

        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();

            string commandString = @$"SELECT * FROM {System.Configuration.ConfigurationManager.AppSettings.Get("DatabaseName")} {whereInject}";
            System.Data.IDataReader reader;
            if (parameters.Count != 0)
            {
                DynamicParameters dynamicParameters = new DynamicParameters(parameters);
                reader = conn.ExecuteReader(commandString, dynamicParameters);
            }
            else
            {
                reader = conn.ExecuteReader(commandString);
            }

            while (reader.Read())
            {
                records.Add(new CodingSession(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetInt32(6), reader.GetString(7), reader.GetInt32(8) == 1 ? true : false));
            }

            conn.Close();
        }

        SortingDetails sortingDetails = filter.SortingDetails;

        if (sortingDetails != null)
        {
            SortingOrder? sortingOrder = sortingDetails.SortOrder;
            SortingBy? sortingBy = sortingDetails.SortBy;

            switch (sortingBy)
            {
                case SortingBy.CreationDate:
                    if (sortingOrder == SortingOrder.Ascending)
                        records = records.OrderBy(x => Helpers.SqlDateToSortableDate(x.CreationDate)).ToList();
                    else
                        records = records.OrderByDescending(x => Helpers.SqlDateToSortableDate(x.CreationDate)).ToList();
                    break;
                case SortingBy.UpdateDate:
                    if (sortingOrder == SortingOrder.Ascending)
                        records = records.OrderBy(x => Helpers.SqlDateToSortableDate(x.LastUpdateDate)).ToList();
                    else
                        records = records.OrderByDescending(x => Helpers.SqlDateToSortableDate(x.LastUpdateDate)).ToList();
                    break;
                case SortingBy.StartDate:
                    if (sortingOrder == SortingOrder.Ascending)
                        records = records.OrderBy(x => Helpers.SqlDateToSortableDate(x.StartDate)).ToList();
                    else
                        records = records.OrderByDescending(x => Helpers.SqlDateToSortableDate(x.StartDate)).ToList();
                    break;
                case SortingBy.EndDate:
                    if (sortingOrder == SortingOrder.Ascending)
                        records = records.OrderBy(x => Helpers.SqlDateToSortableDate(x.EndDate)).ToList();
                    else
                        records = records.OrderByDescending(x => Helpers.SqlDateToSortableDate(x.EndDate)).ToList();
                    break;
                case SortingBy.Duration:
                    if (sortingOrder == SortingOrder.Ascending)
                        records = records.OrderBy(x => x.Duration).ToList();
                    else
                        records = records.OrderByDescending(x => x.Duration).ToList();
                    break;
                case SortingBy.NumberOfLines:
                    if (sortingOrder == SortingOrder.Ascending)
                        records = records.OrderBy(x => x.NumberOfLines).ToList();
                    else
                        records = records.OrderByDescending(x => x.NumberOfLines).ToList();
                    break;
                case SortingBy.Comment:
                    if (sortingOrder == SortingOrder.Ascending)
                        records = records.OrderBy(x => x.Comments).ToList();
                    else
                        records = records.OrderByDescending(x => x.Comments).ToList();
                    break;
                case SortingBy.WasTimerTracked:
                    if (sortingOrder == SortingOrder.Ascending)
                        records = records.OrderBy(x => x.WasTimerTracked).ToList();
                    else
                        records = records.OrderByDescending(x => x.WasTimerTracked).ToList();
                    break;
            }
        }

        CurrentSessions = records;

        return records;
    }
    internal static void DeleteRecords(List<int> index)
    {
        using (SqliteConnection conn = new SqliteConnection(Settings.ConnectionString))
        {
            conn.Open();
            foreach (int i in index)
            {
                string command = @$"DELETE FROM '{System.Configuration.ConfigurationManager.AppSettings.Get("DatabaseName")}' WHERE @id = Id";
                conn.Execute(command, new { id = i });
            }
            conn.Close();
        }
    }
}

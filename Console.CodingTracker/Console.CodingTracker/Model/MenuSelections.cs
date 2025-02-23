namespace Console.CodingTracker.MenuSelections;

internal enum MainMenu
{
    AddRecordManually,
    StartTrackingNewSession,
    SetGoals,
    ViewPreviousSessions,
    GenerateReport,
    UpdateSessionDetails,
    DeleteSession,
    ExitApp
}
internal enum TrackNewSession
{
    AddSessionStart,
    AddSessionEnd,
    OptionalAddSessionNumberOfLines,
    OptionalAddSessionComments,
    ReturnToMainMenu
}
internal enum FilterRecords
{
    ClearFilters,
    TableViewOptions,
    OptionalSorting,
    OptionalFromDate,
    OptionalToDate,
    OptionalMinimalNumberOfLines,
    OptionalMaximalNumberOfLines,
    OptionalComment,
    OptionalMinimalDuration,
    OptionalMaximalDuration,
    OptionalWasTimerTracked,
    ReturnToMainMenu
}
internal enum FilterRecordsForReport
{
    ClearFilters,
    OptionalFromDate,
    OptionalToDate,
    OptionalMinimalNumberOfLines,
    OptionalMaximalNumberOfLines,
    OptionalComment,
    OptionalMinimalDuration,
    OptionalMaximalDuration,
    OptionalWasTimerTracked,
}
internal enum SortingOrder
{
    Ascending,
    Descending
}
internal enum SortingBy
{
    CreationDate,
    UpdateDate,
    StartDate,
    EndDate,
    Duration,
    NumberOfLines,
    Comment,
    WasTimerTracked
}
internal enum SortingMenu
{
    Clear,
    SortingOrder,
    SortingBy,
    Cancel
}
internal enum RecordSessionStartMenu
{
    StartTrackingNow,
    ReturnToMainMenu
}
internal enum RecordSessionRecording
{
    Pause,
    Discard,
    Finish
}
internal enum RecordSessionPause
{
    Unpause,
    Discard,
    Finish
}
internal enum UpdateMenu
{
    UpdateStartDate,
    UpdateEndDate,
    UpdateNumberOfLines,
    UpdateComments,
    BackToFilterMenu,
    BackToMainMenu,
}
internal enum DeletionMenu
{
    Delete,
    BackToFileterMenu,
    BackToMainMenu
}
internal enum TableViewMenu
{
    DisplayCreationDate,
    DisplayLastUpdateDate,
    DisplayStartDate,
    DisplayEndDate,
    DisplayDuration,
    DisplayNumberOfLines,
    DisplayComments,
    DisplayIfWasTimerTracked
}
internal enum TimerTracked
{
    TimerTracked,
    InsertedManually
}

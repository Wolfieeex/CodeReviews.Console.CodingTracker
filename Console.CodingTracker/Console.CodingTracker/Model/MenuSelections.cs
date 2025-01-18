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
    OptionalSorting,
    OptionalFromDate,
    OptionalToDate,
    OptionalMinimalNumberOfLines,
    OptionalMaximalNumberOfLines,
    OptionalComment,
    OptionalMinimalDuration,
    OptionalMaximalDuration,
    ReturnToMainMenu
}

internal enum FilteringOrder
{
    Ascending,
    Descending
}

internal enum FilteringOrderOption
{
    Execute,
    Clear,
    Order,
    CreationDate,
    UpdateDate,
    StartDate,
    EndDate,
    Duration,
    Line,
    Comment,
    ReturnToFilterMenu
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
    OptionalUpdateStartDate,
    OptionalUpdateEndDate,
    OptionalUpdateNumberOfLines,
    OptionalUpdateComments,
    BackToRecordSelection,
    BackToFilterMenu,
    BackToMainMenu,
}

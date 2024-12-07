namespace Console.CodingTracker.MenuSelections;

internal enum MainMenu
{
    AddRecordManually,
    StartTrackingNewSession,
    SetGoals,
    ViewPreviousSessions,
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
    OptionalFromDate,
    OptionalToDate,
    OptionalMinimalNumberOfLines,
    OptionalMaximalNumberOfLines,
    OptionalComment,
    OptionalMinimalDuration,
    OptionalMaximalDuration,
    ReturnToMainMenu
}

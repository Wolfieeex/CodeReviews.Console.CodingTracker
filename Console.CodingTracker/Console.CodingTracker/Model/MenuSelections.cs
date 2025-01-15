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
    ClearFilters,
    OptionalFromDate,
    OptionalToDate,
    OptionalMinimalNumberOfLines,
    OptionalMaximalNumberOfLines,
    OptionalComment,
    OptionalMinimalDuration,
    OptionalMaximalDuration,
    ReturnToMainMenu
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

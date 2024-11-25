namespace Console.CodingTracker.MenuSelections;

internal enum MainMenu
{
    TrackNewSession,
    SetGoals,
    ViewPreviousSessions,
    UpdateSessionDetails,
    DeleteSession,
    ExitApp
}

internal enum TrackNewSession
{
    StartTrackingNow,
    AddSessionStart,
    AddSessionEnd,
    OptionalAddSessionNumberOfLines,
    OptionalAddSessionComments,
}

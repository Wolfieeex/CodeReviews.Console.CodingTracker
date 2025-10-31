using System.ComponentModel.DataAnnotations;


namespace CodingTracker.Wolfieeex.Model;

internal enum MainMenuSelections
{
    [Display(Name = "Add record manually")]
    AddRecordManually,

    [Display(Name = "Start tracking new session")]
    StartTrackingNewSession,

    [Display(Name = "Set goals")]
    SetGoals,

    [Display(Name = "View previous sessions")]
    ViewPreviousSessions,

    [Display(Name = "Generate report")]
    GenerateReport,

    [Display(Name = "Update session details")]
    UpdateSessionDetails,
    
    [Display(Name = "Delete session")]
    DeleteSession,

    [Display(Name = "Exit application")]
    ExitApp
}
internal enum TrackNewSession
{
    [Display(Name = "Add a new session with below details")]
    [EnumSpecialLabel(label: MultiInputLabel.Confirm)]
    Confirm,

    [Display(Name = "Choose the start date of the session")]
    [EnumSpecialLabel(label: MultiInputLabel.Required, shortName: "Session Start")]
    AddSessionStart,

    [Display(Name = "Choose the end date of the session")]
    [EnumSpecialLabel(label: MultiInputLabel.Required, shortName: "Session End")]
    AddSessionEnd,

    [Display(Name = "Enter the approximate number of lines you changed in your program")]
    [EnumSpecialLabel(label: MultiInputLabel.Neutral, shortName: "Lines Changed")]
    AddSessionNumberOfLines,

    [Display(Name = "Add any comments to your session")]
    [EnumSpecialLabel(label: MultiInputLabel.Neutral, shortName: "Comments")]
    AddSessionComments,

    [Display(Name = "Return to previous menu")]
    [EnumSpecialLabel(label: MultiInputLabel.Quit)]
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
internal enum SortingMenu
{
    Clear,
    SortingOrder,
    SortingBy,
    Cancel
}
internal enum GoalSetterMenu
{
    ReturnToMainMenu,
    SetNewGoal,
    ViewPreviousGoals,
    DeleteGoal
}

internal enum GoalViewerMenu
{
    ReturnToPreviousMenu,
    ShowInProgressGoals,
    ShowCompletedGoals,
    ShowFailedGoals,
    ClearHistory
}

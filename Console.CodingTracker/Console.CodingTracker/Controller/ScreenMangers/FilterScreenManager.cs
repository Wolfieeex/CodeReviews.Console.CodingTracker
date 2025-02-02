﻿using Console.CodingTracker.Model;
using Console.CodingTracker.View;
using Spectre.Console;

namespace Console.CodingTracker.Controller.ScreenMangers;

internal class FilterScreenManager
{
    internal static FilterDetails BasicFilterMenu(string preTitle, ref bool returnToMenu, ref FilterDetails filterDetails, ref SortingDetails sortingDetails, ref bool runFilterMenuLoop, Dictionary<string, string> dic, string reason, bool shouldBlock)
    {
        int? userOption = UserInterface.DisplaySelectionUIWithUserInputs(preTitle + "Select [purple]filters[/] for your search:", typeof(MenuSelections.FilterRecords), Color.Plum2, dic, "[green]SearchRecords[/]", shouldBlock, reason);

        string temp = "";
        switch (userOption)
        {
            case -1:
                TemporaryData.lastFilter = filterDetails;
                return filterDetails;
            case 0:
                if (UserInterface.DisplayConfirmationSelectionUI("Are you sure you want to remove all your previous filters?", "Yes", "No"))
                {
                    sortingDetails = new SortingDetails()
                    {
                        SortBy = null,
                        SortOrder = null
                    };
                    filterDetails = new FilterDetails()
                    {
                        SortingDetails = new SortingDetails() { SortBy = null, SortOrder = null },
                        ViewOptions = new bool[] { false, false, false, false, true, true, true, false },
                        FromDate = null,
                        ToDate = null,
                        MinLines = null,
                        MaxLines = null,
                        Comment = null,
                        MinDuration = null,
                        MaxDuration = null,
                        WasTimerTracked = null
                    };
                }
                break;
            case 1:
                bool[] tempViewOptions = filterDetails.ViewOptions;
                UserInterface.DisplayMultiselectionUI("Please [yellow4]toggle select elements you want your datatable to include[/]:", typeof(MenuSelections.TableViewMenu), ref tempViewOptions);
                filterDetails.ViewOptions = tempViewOptions;
                break;
            case 2:
                sortingDetails = CRUDController.SortingMenu(sortingDetails);
                break;
            case 3:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]the date from which you want to search[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.DateOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.FromDate = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 4:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]the date to which you want to search[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.DateOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.ToDate = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 5:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]the minimal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MinLines = string.IsNullOrEmpty(temp) || temp == "" ? temp : (Int32.Parse(temp) < 1 ? "1" : temp.Trim());
                break;
            case 6:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]the maximal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MaxLines = string.IsNullOrEmpty(temp) || temp == "" ? temp : (Int32.Parse(temp) < 1 ? "1" : temp.Trim());
                break;
            case 7:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]part of the comment[/] you want to search for. ", TextUIOptions.Optional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.Comment = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 8:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]minimal duration[/] of the sessions you want to search for in \"d hh:mm\" format. ", TextUIOptions.TimeSpanOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MinDuration = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 9:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]maximal duration[/] of the sessions you want to search for in \"d hh:mm\" format. ", TextUIOptions.TimeSpanOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MaxDuration = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 10:
                dynamic resultSortingOrder = UserInterface.DisplayEnumSelectionUI("Show records that were [purple]timer tracked (true)[/], or were [purple]inserted manually (false)[/]: ", typeof(MenuSelections.TimerTracked), Color.MediumPurple);
                if (!(resultSortingOrder is int))
                {
                    filterDetails.WasTimerTracked = Enum.GetName(resultSortingOrder) == "TimerTracked" ? "True" : "False";
                }
                else
                {
                    filterDetails.WasTimerTracked = null;
                }
                break;
            case 11:
                returnToMenu = true;
                runFilterMenuLoop = false;
                break;
        }
        return null;
    }
    internal static FilterDetails ReportFilterMenu(string preTitle, ref bool returnToMenu, ref FilterDetails filterDetails, ref SortingDetails sortingDetails, ref bool runFilterMenuLoop, Dictionary<string, string> dic, string reason, bool shouldBlock)
    {
        int? userOption = UserInterface.DisplaySelectionUIWithUserInputs(preTitle + "Select [purple]filters[/] for your search:", typeof(MenuSelections.FilterRecords), Color.Plum2, dic, "[green]SearchRecords[/]", shouldBlock, reason);

        string temp = "";
        switch (userOption)
        {
            case -1:
                TemporaryData.lastFilter = filterDetails;
                return filterDetails;
            case 0:
                if (UserInterface.DisplayConfirmationSelectionUI("Are you sure you want to remove all your previous filters?", "Yes", "No"))
                {
                    sortingDetails = new SortingDetails()
                    {
                        SortBy = null,
                        SortOrder = null
                    };
                    filterDetails = new FilterDetails()
                    {
                        SortingDetails = new SortingDetails() { SortBy = null, SortOrder = null },
                        ViewOptions = new bool[] { false, false, false, false, true, true, true, false },
                        FromDate = null,
                        ToDate = null,
                        MinLines = null,
                        MaxLines = null,
                        Comment = null,
                        MinDuration = null,
                        MaxDuration = null,
                        WasTimerTracked = null
                    };
                }
                break;
            case 1:
                bool[] tempViewOptions = filterDetails.ViewOptions;
                UserInterface.DisplayMultiselectionUI("Please [yellow4]toggle select elements you want your datatable to include[/]:", typeof(MenuSelections.TableViewMenu), ref tempViewOptions);
                filterDetails.ViewOptions = tempViewOptions;
                break;
            case 2:
                sortingDetails = CRUDController.SortingMenu(sortingDetails);
                break;
            case 3:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]the date from which you want to search[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.DateOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.FromDate = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 4:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]the date to which you want to search[/] in \"dd/mm/yyyy, hh:mm\" format. ", TextUIOptions.DateOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.ToDate = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 5:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]the minimal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MinLines = string.IsNullOrEmpty(temp) || temp == "" ? temp : (Int32.Parse(temp) < 1 ? "1" : temp.Trim());
                break;
            case 6:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]the maximal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MaxLines = string.IsNullOrEmpty(temp) || temp == "" ? temp : (Int32.Parse(temp) < 1 ? "1" : temp.Trim());
                break;
            case 7:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]part of the comment[/] you want to search for. ", TextUIOptions.Optional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.Comment = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 8:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]minimal duration[/] of the sessions you want to search for in \"d hh:mm\" format. ", TextUIOptions.TimeSpanOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MinDuration = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 9:
                temp = UserInterface.DisplayTextUI("Please insert [Blue]maximal duration[/] of the sessions you want to search for in \"d hh:mm\" format. ", TextUIOptions.TimeSpanOnlyOptional);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MaxDuration = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 10:
                dynamic resultSortingOrder = UserInterface.DisplayEnumSelectionUI("Show records that were [purple]timer tracked (true)[/], or were [purple]inserted manually (false)[/]: ", typeof(MenuSelections.TimerTracked), Color.MediumPurple);
                if (!(resultSortingOrder is int))
                {
                    filterDetails.WasTimerTracked = Enum.GetName(resultSortingOrder) == "TimerTracked" ? "True" : "False";
                }
                else
                {
                    filterDetails.WasTimerTracked = null;
                }
                break;
            case 11:
                returnToMenu = true;
                runFilterMenuLoop = false;
                break;
        }
        return null;
    }
}

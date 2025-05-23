﻿using Console.CodingTracker.Controller.CRUD;
using Console.CodingTracker.Model;
using Console.CodingTracker.View;
using Spectre.Console;

namespace Console.CodingTracker.Controller.ScreenMangers;

internal enum TimerTracked
{
    TimerTracked,
    InsertedManually
}

internal class FilterScreenManager
{
    internal static void BasicFilterMenu(string preTitle, ref bool returnToMenu, ref FilterDetails filterDetails, ref SortingDetails sortingDetails, ref bool runFilterMenuLoop, Dictionary<string, string> dic, string reason, bool shouldBlock, Color titleColor, Color mainColor, Color inputColor)
    {
        int? userOption = UserInterface.DisplaySelectionUIWithUserInputs(preTitle + $"Select [#{titleColor.ToHex()}]filters[/] for your search:", typeof(MenuSelections.FilterRecords), titleColor, mainColor, inputColor, dic, "[green]SearchRecords[/]", shouldBlock, reason);

        string temp = "";
        switch (userOption)
        {
            case -1:
                TemporaryData.LastFilter = filterDetails;
                runFilterMenuLoop = false;
                break;
            case 0:
                if (UserInterface.DisplayConfirmationSelectionUI("Are you sure you want to remove all your previous filters?", "Yes", "No", inputColor))
                {
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
                    TemporaryData.LastFilter = filterDetails;
                }
                break;
            case 1:
                bool[] tempViewOptions = filterDetails.ViewOptions;
                UserInterface.DisplayMultiselectionUI($"Please [#{titleColor.ToHex()}]toggle select elements you want your datatable to include[/]:", typeof(MenuSelections.TableViewMenu), ref tempViewOptions, mainColor);
                filterDetails.ViewOptions = tempViewOptions;
                break;
            case 2:
                filterDetails.SortingDetails = FilterController.SortingMenu(sortingDetails, titleColor, mainColor, inputColor);
                break;
            case 3:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]the date from which you want to search[/] in [#{titleColor.ToHex()}]\"dd/mm/yyyy, hh:mm\"[/] format. ", TextUIOptions.DateOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.FromDate = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 4:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]the date to which you want to search[/] in [#{titleColor.ToHex()}]\"dd/mm/yyyy, hh:mm\"[/] format. ", TextUIOptions.DateOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.ToDate = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 5:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]the minimal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MinLines = string.IsNullOrEmpty(temp) || temp == "" ? temp : (Int32.Parse(temp) < 1 ? "1" : temp.Trim());
                break;
            case 6:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]the maximal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MaxLines = string.IsNullOrEmpty(temp) || temp == "" ? temp : (Int32.Parse(temp) < 1 ? "1" : temp.Trim());
                break;
            case 7:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]part of the comment[/] you want to search for. ", TextUIOptions.Optional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.Comment = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 8:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]minimal duration[/] of the sessions you want to search for in [#{titleColor.ToHex()}]\"d hh:mm\"[/] format. ", TextUIOptions.TimeSpanOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MinDuration = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 9:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]maximal duration[/] of the sessions you want to search for in [#{titleColor.ToHex()}]\"d hh:mm\"[/] format. ", TextUIOptions.TimeSpanOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MaxDuration = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 10:
                dynamic resultSortingOrder = UserInterface.DisplayEnumSelectionUI($"Show records that were [#{titleColor.ToHex()}]timer tracked[/], or were [#{titleColor.ToHex()}]inserted manually[/]: ", typeof(TimerTracked), mainColor);
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
    }
    internal static void ReportFilterMenu(string preTitle, ref FilterDetails filterDetails, ref bool runFilterMenuLoop, Dictionary<string, string> dic, string reason, bool shouldBlock, Color titleColor, Color mainColor, Color inputColor)
    {
        int? userOption = UserInterface.DisplaySelectionUIWithUserInputs(preTitle + $"Select [{titleColor}]filters[/] for your search:", typeof(MenuSelections.FilterRecordsForReport), titleColor, mainColor, inputColor, dic, "[green]Confirm filters[/]", shouldBlock, reason);

        string temp = "";
        switch (userOption)
        {
            case -1:
                TemporaryData.LastFilter = filterDetails;
                runFilterMenuLoop = false;
                break;
            case 0:
                if (UserInterface.DisplayConfirmationSelectionUI("Are you sure you want to remove all your previous filters?", "Yes", "No", inputColor))
                {
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
                    TemporaryData.LastFilter = filterDetails;
                }
                break;
            case 1:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]the date from which you want to search[/] in [#{titleColor.ToHex()}]\"dd/mm/yyyy, hh:mm\"[/] format. ", TextUIOptions.DateOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.FromDate = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 2:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]the date to which you want to search[/] in [#{titleColor.ToHex()}]\"dd/mm/yyyy, hh:mm\"[/] format. ", TextUIOptions.DateOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.ToDate = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 3:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]the minimal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MinLines = string.IsNullOrEmpty(temp) || temp == "" ? temp : (Int32.Parse(temp) < 1 ? "1" : temp.Trim());
                break;
            case 4:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]the maximal number of lines[/] for searched sessions. ", TextUIOptions.NumbersOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MaxLines = string.IsNullOrEmpty(temp) || temp == "" ? temp : (Int32.Parse(temp) < 1 ? "1" : temp.Trim());
                break;
            case 5:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]part of the comment[/] you want to search for. ", TextUIOptions.Optional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.Comment = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 6:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]minimal duration[/] of the sessions you want to search for in [#{titleColor.ToHex()}]\"d hh:mm\"[/] format. ", TextUIOptions.TimeSpanOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MinDuration = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 7:
                temp = UserInterface.DisplayTextUI($"Please insert [#{titleColor.ToHex()}]maximal duration[/] of the sessions you want to search for in [#{titleColor.ToHex()}]\"d hh:mm\"[/] format. ", TextUIOptions.TimeSpanOnlyOptional, mainColor);
                if (temp == "e")
                {
                    break;
                }
                filterDetails.MaxDuration = string.IsNullOrEmpty(temp) ? temp : temp.Trim();
                break;
            case 8:
                dynamic resultSortingOrder = UserInterface.DisplayEnumSelectionUI($"Show records that were [#{titleColor.ToHex()}]timer tracked (true)[/], or were [#{titleColor.ToHex()}]inserted manually (false)[/]: ", typeof(TimerTracked), Color.MediumPurple);
                if (!(resultSortingOrder is int))
                {
                    filterDetails.WasTimerTracked = Enum.GetName(resultSortingOrder) == "TimerTracked" ? "True" : "False";
                }
                else
                {
                    filterDetails.WasTimerTracked = null;
                }
                break;
        }
    }
}

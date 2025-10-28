using CodingTracker.Wolfieeex.Model;
using Spectre.Console;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace CodingTracker.Wolfieeex.View;

internal abstract class MultiInputMenu : Menu
{
    public MultiInputMenu(Color color) : base(color) { }
    protected Dictionary<Enum, string> OptionKeys = new();
    protected string[] reasonCodes;
    protected virtual bool CheckInputConditions()
    {
        // Special conditions, such as From date not being after To date.
        return false;
    }
    protected override Enum DisplayOptions()
    {
        try
        {
            // Do the actual display function from Ansi Console and return the selection
            List<Enum> options = GenerateOptions();

            return AnsiConsole.Prompt(new SelectionPrompt<Enum>()
            .Title(title)
            .AddChoices(options)
            .UseConverter(s => ReadEnumName(s))
            .HighlightStyle(style)
            .WrapAround()
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return null;
    }
    protected List<Enum> GenerateOptions()
    {
        List<Enum> allEnums = Enum.GetValues(selectionEnum).Cast<Enum>().ToList();

        bool conditionsPassed = CheckInputConditions();
        bool checkPassed = true;

        if (conditionsPassed)
        {
            bool oneOfPresent = false;
            bool oneOfSatisfied = false;

            foreach (Enum en in allEnums)
            {
                MultiInputLabel label = ReadEnumSpecialLabel(en);
                if (label == MultiInputLabel.Required && !OptionKeys.ContainsKey(en))
                {
                    checkPassed = false;
                    break;
                }
                if (label == MultiInputLabel.OneOfRequired)
                {
                    oneOfPresent = true;
                    if (OptionKeys.ContainsKey(en))
                    {
                        oneOfSatisfied = true;
                    }
                }
            }
            if (oneOfPresent && !oneOfSatisfied)
                checkPassed = false;
        }

        foreach (Enum en in allEnums)
        {
            MultiInputLabel label = ReadEnumSpecialLabel(en);
            if (label == MultiInputLabel.Confirm && checkPassed && conditionsPassed)
                allEnums.Add(en);
            else if (label != MultiInputLabel.Confirm)
                allEnums.Add(en);
        }
        return allEnums;
    }

    protected void AlterKey(Enum key, string? value)
    {
        // If 'optionKey' exists, change. If not, add. If null, remove. If empty, don't change.

        if (OptionKeys.ContainsKey(key))
        {
            if (value == null)
            {
                OptionKeys.Remove(key);
            }
            else if (value == "") { }
            else
            {
                OptionKeys[key] = value;
            }
        }
        else
        {
            if (value == null) { }
            else if (value == "") { }
            else
            {
                OptionKeys.Add(key, value);
            }
        }
    }
    
    protected override string ReadEnumName(Enum enumValue)
    {
        MemberInfo? memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
        if (memberInfo != null)
        {
            var attribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Name))
            {
                if (OptionKeys.ContainsKey(enumValue))
                    return attribute.Name + ": " + OptionKeys[enumValue].ToString();
                else
                    return attribute.Name;
            }
        }
        return enumValue.ToString();
    }
}
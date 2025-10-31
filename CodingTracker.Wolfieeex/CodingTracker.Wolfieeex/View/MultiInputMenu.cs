using CodingTracker.Wolfieeex.Model;
using Spectre.Console;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace CodingTracker.Wolfieeex.View;

internal abstract class MultiInputMenu : Menu
{
    public MultiInputMenu(Color color) : base(color) { }
    protected Dictionary<Enum, string> OptionKeys = new();
    protected List<string> reasonCodes;
    protected string titleWithReasons => title;
    protected virtual bool CheckInputConditions()
    {
        bool checkPassed = true;
        List<string> requiredFields = new();
        List<string> oneOfRequiredFields = new();

        List<Enum> keys = OptionKeys.Keys.ToList();
        List<Enum> selectionEnumOptions = Enum.GetValues(selectionEnum).Cast<Enum>().ToList();
        Dictionary<Enum, MultiInputLabel> selectionSettings = new();

        bool oneOfExists = false;
        foreach (Enum en in selectionEnumOptions)
        {
            if (ReadEnumSpecialLabel(en) == MultiInputLabel.OneOfRequired)
            {
                oneOfExists = true;
                break;
            }
        }

        // Check for every required label and if not present in keys, add it to required fields query string.
        // Check for at least one OneOf required label and if none present in keys, add them all to the query string.
        
        // Method can be overriten by calling out base + additional checks if needed.

        return false;
    }
    protected override Enum DisplayOptions()
    {
        try
        {
            if (!selectionEnum.IsEnum)
                throw new ArgumentException("DisplayOptions method can only accept types of enum type. " +
                "Make sure that selectionEnum variable is set to Enum in Menu abstract class.");

            // Do the actual display function from Ansi Console and return the selection
            List<Enum> options = GenerateOptions();

            return AnsiConsole.Prompt(new SelectionPrompt<Enum>()
            .Title(titleWithReasons)
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

        bool didConditionsPass = CheckInputConditions();

        // Generate the title.

        foreach (Enum en in allEnums)
        {
            if (ReadEnumSpecialLabel(en) == MultiInputLabel.Confirm && didConditionsPass)
                allEnums.Add(en);
            else if (ReadEnumSpecialLabel(en) != MultiInputLabel.Confirm)
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

    protected string ReadEnumShortName(Enum enumValue)
    {
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        if (fieldInfo != null)
        {
            var attribute = fieldInfo.GetCustomAttribute<EnumSpecialLabel>();
            string shortName = attribute.shortName;

            if (!string.IsNullOrEmpty(shortName))
                return shortName;
        }
        return enumValue.ToString();
    }
}
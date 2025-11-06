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
    protected string titleWithReasons => reasonCodes.Count() > 0 ?
                                         title + $"{menuColorsHex.titleWarningColor}Attention![/] To continue, you need to resolve below issues:\n" + 
                                         "\n" + string.Join("\n\t", reasonCodes)
                                         : title;

    protected virtual bool CheckInputConditions()
    {
        bool checkPassed = true;
        List<string> requiredFields = new();
        List<string> oneOfRequiredFields = new();

        List<Enum> selectionEnumOptions = Enum.GetValues(selectionEnum).Cast<Enum>().ToList();

        bool oneOfRequirement = true;
        foreach (Enum en in selectionEnumOptions)
        {
            if (ReadEnumSpecialLabel(en) == MultiInputLabel.OneOfRequired)
            {
                oneOfRequirement = false;
                oneOfRequiredFields.Add(ReadEnumShortName(en));
                break;
            }
        }
        foreach (Enum en in selectionEnumOptions)
        {
            if (ReadEnumSpecialLabel(en) == MultiInputLabel.Required && !selectionEnumOptions.Contains(en))
            {
                checkPassed = false;
                requiredFields.Add(ReadEnumShortName(en));
            }
            else if (ReadEnumSpecialLabel(en) == MultiInputLabel.OneOfRequired && !oneOfRequirement && selectionEnumOptions.Contains(en))
            {
                oneOfRequiredFields.Clear();
                oneOfRequirement = true;
            }
        }
        reasonCodes.Clear();

        if (requiredFields.Count() != 0)
            reasonCodes.Add($"{menuColorsHex.titleHighlightColor}All required fields must be filled in[/]: "
            + $"{menuColorsHex.titleHighlightColor}{string.Join(", ", requiredFields)}.[/]");

        if (!oneOfRequirement)
            reasonCodes.Add($"{menuColors.titleHighlightColor}You need to fill in at least one of required fields[/]: "
            + $"{menuColorsHex.titleHighlightColor}{string.Join(", ", oneOfRequiredFields)}.[/]");

        // Method can be overriten by calling out base + additional checks if needed and return combined bool result in "&&" form;
        checkPassed = checkPassed && oneOfRequirement;
        return checkPassed;
    }
   
    protected override Enum DisplayOptions()
    {
        try
        {
            if (!selectionEnum.IsEnum)
                throw new ArgumentException("DisplayOptions method can only accept types of enum type. " +
                "Make sure that selectionEnum variable is set to Enum in Menu abstract class.");

            return AnsiConsole.Prompt(new SelectionPrompt<Enum>()
            .Title(titleWithReasons)
            .AddChoices(GenerateOptions())
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

        foreach (Enum en in allEnums)
        {
            if (ReadEnumSpecialLabel(en) == MultiInputLabel.Confirm && didConditionsPass)
                allEnums.Add(en);
            else if (ReadEnumSpecialLabel(en) != MultiInputLabel.Confirm)
                allEnums.Add(en);
        }
        return allEnums;
    }

    protected string AlterKey(Enum key, string? value)
    {
        // If 'optionKey' exists, change. If not, add. If null, remove. If empty, don't change.
        if (OptionKeys.ContainsKey(key))
        {
            if (value == null)
                OptionKeys.Remove(key);
            else if (value != "")
                OptionKeys[key] = value;
        }
        else
        {
            if (value != null && value != "")
                OptionKeys.Add(key, value);
        }
        return value == null ? "" : value;
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
            string shortName = attribute.ShortName;

            if (!string.IsNullOrEmpty(shortName))
                return shortName;
        }
        return enumValue.ToString();
    }
}
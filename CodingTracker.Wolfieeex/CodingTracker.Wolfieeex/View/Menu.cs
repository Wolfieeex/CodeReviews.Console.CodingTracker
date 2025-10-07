using System.ComponentModel;
using System.Reflection;
using Spectre.Console;
using CodingTracker.Wolfieeex.Model;
using System.ComponentModel.DataAnnotations;

namespace CodingTracker.Wolfieeex.View;

public struct MenuColors
{
    public Color mainColor;
    public Color titleColor;
    public Color titleHighlightColor;
    public Color titleWarningColor;
    public Color titlePositiveColor;
    public Color selectionColor;
    public Color selection2Color;
}

public struct MenuColorsHex
{
    public string mainColor;
    public string titleColor;
    public string titleHighlightColor;
    public string titleWarningColor;
    public string titlePositiveColor;
    public string selectionColor;
    public string selection2Color;
}

internal abstract class Menu
{
    protected abstract string title { get; }
    protected abstract Type selectionEnum { get; }
    private Color _basicColor;
    protected Color basicColor
    {
        get => _basicColor; set
        {
            menuColors = new MenuColors
            {
                mainColor = value,
                titleColor = value.Blend(Color.Yellow3_1, 0.4f),
                titleHighlightColor = value.Blend(Color.BlueViolet, 0.4f),
                titleWarningColor = value.Blend(Color.Red3_1, 0.4f),
                titlePositiveColor = value.Blend(Color.GreenYellow, 0.4f),
                selectionColor = value.Blend(Color.Blue3_1, 0.4f),
                selection2Color = value.Blend(Color.RosyBrown, 0.4f),
            };

            menuColorsHex = new MenuColorsHex
            {
                mainColor = "[#" + value.ToHex() + "]",
                titleColor = "[#" + value.Blend(Color.Yellow3_1, 0.4f).ToHex() + "]",
                titleHighlightColor = "[#" + value.Blend(Color.BlueViolet, 0.4f).ToHex() + "]",
                titleWarningColor = "[#" + value.Blend(Color.Red3_1, 0.4f).ToHex() + "]",
                titlePositiveColor = "[#" + value.Blend(Color.GreenYellow, 0.4f).ToHex() + "]",
                selectionColor = "[#" + value.Blend(Color.Blue3_1, 0.4f).ToHex() + "]",
                selection2Color = "[#" + value.Blend(Color.RosyBrown, 0.4f).ToHex() + "]",
            };
        }
    }
    protected MenuColors menuColors;
    protected MenuColorsHex menuColorsHex;
    protected Style style { get; }

    public abstract void DisplayMenu();

    public Menu(Color color)
    {
        basicColor = color;
        style = new Style(foreground: menuColors.selectionColor, decoration: Decoration.RapidBlink);
    }

    protected string ReadEnumName(Enum enumValue)
    {
        MemberInfo? memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
        if (memberInfo != null)
        {
            var attribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Name))
                return attribute.Name;
        }
        return enumValue.ToString();
    }

    protected string ReadEnumDescription(Enum enumValue)
    {
        FieldInfo? field = enumValue.GetType().GetField(enumValue.ToString());
        if (field != null)
        {
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Description))
                return attribute.Description;
        }
        return enumValue.ToString();
    }

    protected MultiInputLabel ReadEnumSpecialLabel(Enum enumValue)
    {
        FieldInfo? field = enumValue.GetType().GetField(enumValue.ToString());
        if (field != null)
        {
            var attribute = field.GetCustomAttribute<EnumSpecialLabel>();
            if (attribute != null)
                return attribute.multiInputLabel;
        }
        return MultiInputLabel.Neutral;
    }

    protected Enum DisplayOptions()
    {
        try
        {
            if (!selectionEnum.IsEnum)
                throw new ArgumentException("DisplayOptions method can only accept types of enum type. " +
                "Make sure that selectionEnum variable is set to Enum in Menu abstract class.");

            var enumValues = Enum.GetValues(selectionEnum).Cast<Enum>().ToList();

            return AnsiConsole.Prompt(new SelectionPrompt<Enum>()
            .Title(title)
            .AddChoices(enumValues)
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
}

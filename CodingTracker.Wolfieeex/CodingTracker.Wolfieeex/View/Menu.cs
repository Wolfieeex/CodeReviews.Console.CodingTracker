using System.ComponentModel;
using System.Reflection;
using Spectre.Console;
using CodingTracker.Wolfieeex.Model;

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

internal abstract class Menu
{
    protected string title;
    private Color _basicColor;
    public Color basicColor
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
        }
    }
    public MenuColors menuColors;
    protected Style style;

    public abstract void DisplayMenu();

    public Menu(Color color, Style overrideStyle = null)
    {
        basicColor = color;

        if (overrideStyle != null)
            style = overrideStyle;
        else
            style = new Style(foreground: menuColors.selectionColor, decoration: Decoration.RapidBlink);
    }

    protected string ReadEnumName(Enum enumValue)
    {
        FieldInfo? field = enumValue.GetType().GetField(enumValue.ToString());
        if (field != null)
        {
            var attribute = field.GetCustomAttribute<DisplayNameAttribute>();
            if (attribute != null)
                return attribute.DisplayName;
        }
        return enumValue.ToString();
    }

    protected string ReadEnumDescription(Enum enumValue)
    {
        FieldInfo? field = enumValue.GetType().GetField(enumValue.ToString());
        if (field != null)
        {
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            if (attribute != null)
                return attribute.Description;
        }
        return enumValue.ToString();
    }

    protected MultiInputLabel GetEnumSpecialLabel(Enum enumValue)
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
}

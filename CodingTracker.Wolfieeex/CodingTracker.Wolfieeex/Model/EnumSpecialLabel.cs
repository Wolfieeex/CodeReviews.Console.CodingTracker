namespace CodingTracker.Wolfieeex.Model;

public enum MultiInputLabel
{
    Confirm,
    Quit,
    Neutral,
    Required,
    OneOfRequired
}

public class EnumSpecialLabel : Attribute
{
    public MultiInputLabel multiInputLabel;

    public EnumSpecialLabel(MultiInputLabel label)
    {
        multiInputLabel = label;
    }
}
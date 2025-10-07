namespace CodingTracker.Wolfieeex.Model;

public enum MultiInputLabel
{
    Confirm,
    Quit,
    Neutral,
    Required,
    OneOfRequired
}

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class EnumSpecialLabel : Attribute
{
    public MultiInputLabel multiInputLabel;

    public EnumSpecialLabel(MultiInputLabel label)
    {
        multiInputLabel = label;
    }
}
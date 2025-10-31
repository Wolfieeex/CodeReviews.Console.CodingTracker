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
    public string shortName;

    public EnumSpecialLabel(MultiInputLabel label, string shortName = "")
    {
        multiInputLabel = label;
        this.shortName = shortName;
    }
}
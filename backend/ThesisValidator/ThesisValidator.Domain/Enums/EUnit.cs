namespace ThesisValidator.Domain.Enums;

public enum EUnit
{
    Cm,
    Pt,
    Mm,
    Page
}

public static class EUnitExtensions
{
    public static string ToDisplayString(this EUnit unit) => unit switch
    {
        EUnit.Cm => "cm",
        EUnit.Pt => "pt",
        EUnit.Mm => "mm",
        EUnit.Page => "lk",
        _ => ""
    };
}

using ThesisValidator.Domain.Enums;

namespace ThesisValidator.DAL.Docx;

public static class UnitConverter
{
    public static double TwipsToUnit(long twips, EUnit unit) => unit switch
    {
        EUnit.Cm => TwipsToCm(twips),
        EUnit.Mm => TwipsToMm(twips),
        EUnit.Pt => TwipsToPt(twips),
        _ => throw new ArgumentOutOfRangeException(nameof(unit))
    };

    // TODO: check that conversions are correct.
    private static double TwipsToCm(long twips) => twips / 1440.0 * 2.54;

    private static double HalfPointsToPt(int halfPoints) => halfPoints / 2.0;

    private static double TwipsToMm(long twips) => twips / 1440.0 * 25.4;

    private static double TwipsToPt(long twips) => twips / 20.0;
}

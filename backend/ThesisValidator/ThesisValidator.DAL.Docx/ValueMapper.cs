using DocumentFormat.OpenXml.Wordprocessing;

namespace ThesisValidator.DAL.Docx;

public static class ValueMapper
{
    public static string? MapAlignment(JustificationValues? value)
    {
        if (value == JustificationValues.Both)
        {
            return "justified";
        }

        if (value == JustificationValues.Left)
        {
            return "left";
        }

        if (value == JustificationValues.Right)
        {
            return "right";
        }

        if (value == JustificationValues.Center)
        {
            return "center";
        }

        return null;
    }
}

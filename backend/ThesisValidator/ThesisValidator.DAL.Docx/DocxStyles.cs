namespace ThesisValidator.DAL.Docx;

public static class DocxStyles
{
    public const string Heading = "Heading";
    public const string Heading1 = "Heading1";
    public const string Heading1SyncFusion = "Heading 1";
    public const string Heading2 = "Heading2";
    public const string Heading3 = "Heading3";
    public const string HeadingUnnumbered = "Heading_unnumber";
    public const string Normal = "Normal";

    public static readonly IReadOnlyList<string> AllHeadings =
    [
        Heading, Heading1, Heading2, Heading3, HeadingUnnumbered
    ];

    public static readonly IReadOnlyList<string> Level1Headings =
    [
        Heading, Heading1, HeadingUnnumbered
    ];

    public static readonly IReadOnlyList<string> Level1HeadingsSyncfusion =
    [
        Heading, Heading1, HeadingUnnumbered, Heading1SyncFusion
    ];
}

using System.Globalization;

namespace Applets.Watch;

internal static class ClockFormatting
{
    public const string DefaultFormat = "M/d ddd";
    public static readonly string[] Formats = [DefaultFormat, "yyyy/MM/dd", "yyyy-MM-dd", "M月d日", "M月d日（ddd）", "yyyy年M月d日（ddd）"];
    public static string DateText(DateTime date, string format)
    {
        if (!Formats.Contains(format)) format = DefaultFormat;
        return date.ToString(format, format.Contains('月') ? CultureInfo.GetCultureInfo("ja-JP") : CultureInfo.InvariantCulture);
    }
}

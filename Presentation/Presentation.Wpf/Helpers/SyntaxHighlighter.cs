using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Presentation.Wpf.Helpers;

public static class SyntaxDecorator
{
    private static readonly string[] Keywords =
    {
        "class", "public", "private", "protected", "interface", "struct",
        "void", "string", "int", "decimal", "bool", "return", "package", "import", "type", "func"
    };

    public static void Highlight(FlowDocument doc)
    {
        var textRange = new TextRange(doc.ContentStart, doc.ContentEnd);
        textRange.ClearAllProperties();

        var text = textRange.Text;

        foreach (var word in Keywords)
        {
            var matches = Regex.Matches(text, $@"\b{word}\b");
            foreach (Match match in matches)
            {
                var start = GetTextPointerAtOffset(doc.ContentStart, match.Index);
                var end = GetTextPointerAtOffset(doc.ContentStart, match.Index + match.Length);

                if (start != null && end != null)
                {
                    var range = new TextRange(start, end);
                    range.ApplyPropertyValue(TextElement.ForegroundProperty,
                        new SolidColorBrush(Color.FromRgb(86, 156, 214)));
                    range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }
            }
        }
    }

    private static TextPointer? GetTextPointerAtOffset(TextPointer start, int offset)
    {
        var current = start;
        var count = 0;
        while (current != null)
        {
            if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                var text = current.GetTextInRun(LogicalDirection.Forward);
                if (count + text.Length >= offset)
                    return current.GetPositionAtOffset(offset - count);

                count += text.Length;
            }

            current = current.GetNextContextPosition(LogicalDirection.Forward);
        }

        return null;
    }
}
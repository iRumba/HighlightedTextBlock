using System.Windows;
using System.Windows.Documents;

namespace HighlightedTextBlock
{
    public abstract class Highlight : DependencyObject
    {
        public abstract void SetHighlight(Span span);

        public abstract void SetHighlight(TextRange range);
    }
}

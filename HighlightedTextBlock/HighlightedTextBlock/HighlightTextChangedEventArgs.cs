using System;

namespace HighlightedTextBlock
{
    public class HighlightTextChangedEventArgs : EventArgs
    {
        public string OldText { get; }

        public string NewText { get; }

        public HighlightTextChangedEventArgs(string oldText, string newText)
        {
            OldText = oldText;
            NewText = newText;
        }
    }
}

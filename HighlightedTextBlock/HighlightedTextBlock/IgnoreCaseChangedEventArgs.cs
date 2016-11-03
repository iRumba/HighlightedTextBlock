using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighlightedTextBlock
{
    public class IgnoreCaseChangedEventArgs : EventArgs
    {
        public bool OldValue { get; }

        public bool NewValue { get; }

        public IgnoreCaseChangedEventArgs(bool oldValue, bool newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}

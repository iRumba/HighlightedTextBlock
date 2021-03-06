﻿using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace HighlightedTextBlock
{
    public class HighlightForeground : Highlight
    {
        public override void SetHighlight(Span span)
        {
            Brush brush = null;
            Application.Current.Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                brush = Brush;
            })).Wait();
            span.Foreground = brush;
        }

        public override void SetHighlight(TextRange range)
        {
            Brush brush = null;
            Application.Current.Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                brush = Brush;
            })).Wait();
            range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        }

        /// <summary>
        /// Кисть для цвета текста
        /// </summary>
        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush", typeof(Brush), typeof(HighlightForeground), new PropertyMetadata(Brushes.Black));
    }
}

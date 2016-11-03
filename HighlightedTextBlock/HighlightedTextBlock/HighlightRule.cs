using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;

namespace HighlightedTextBlock
{
    public class HighlightRule : FrameworkElement
    {
        public delegate void HighlightTextChangedEventHandler(object sender, HighlightTextChangedEventArgs e);
        public delegate void IgnoreCaseChangedEventHandler(object sender, IgnoreCaseChangedEventArgs e);

        public event HighlightTextChangedEventHandler HighlightTextChanged;
        public event IgnoreCaseChangedEventHandler IgnoreCaseChanged;

        public HighlightRule()
        {
            Highlights = new ObservableCollection<Highlight>();
        }

        /// <summary>
        /// Текст, который нужно подсветить
        /// </summary>
        public string HightlightedText
        {
            get { return (string)GetValue(HightlightedTextProperty); }
            set { SetValue(HightlightedTextProperty, value); }
        }

        public static readonly DependencyProperty HightlightedTextProperty =
            DependencyProperty.Register("HightlightedText", typeof(string), typeof(HighlightRule), new FrameworkPropertyMetadata(string.Empty, HighlightPropertyChanged));

        static void HighlightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as HighlightRule;
            if (me != null)
                me.HighlightTextChanged?.Invoke(me, new HighlightTextChangedEventArgs((string)e.OldValue, (string)e.NewValue));
        }

        /// <summary>
        /// Игнорировать регистр? 
        /// </summary>
        public bool IgnoreCase
        {
            get { return (bool)GetValue(IgnoreCaseProperty); }
            set { SetValue(IgnoreCaseProperty, value); }
        }

        public static readonly DependencyProperty IgnoreCaseProperty =
            DependencyProperty.Register("IgnoreCase", typeof(bool), typeof(HighlightRule), new FrameworkPropertyMetadata(true, IgnoreCasePropertyChanged));

        static void IgnoreCasePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as HighlightRule;
            if (me != null)
                me.IgnoreCaseChanged?.Invoke(me, new IgnoreCaseChangedEventArgs((bool)e.OldValue, (bool)e.NewValue));
        }

        /// <summary>
        /// Коллекция подсветок
        /// </summary>
        public ObservableCollection<Highlight> Highlights
        {
            get
            {
                return (ObservableCollection<Highlight>)GetValue(HighlightsProperty);
            }
            set { SetValue(HighlightsProperty, value); }
        }

        public static readonly DependencyProperty HighlightsProperty =
            DependencyProperty.Register("Highlights", typeof(ObservableCollection<Highlight>), typeof(HighlightRule), new PropertyMetadata(null));
    }
}

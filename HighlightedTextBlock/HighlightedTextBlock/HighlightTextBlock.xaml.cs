using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace HighlightedTextBlock
{
    /// <summary>
    /// Логика взаимодействия для HighlightTextBlock.xaml
    /// </summary>
    public partial class HighlightTextBlock : TextBlock
    {
        // Здесь сохраняется сериализованное оригинальное наполнение TextBlock 
        // (подсветка накладывается на оригинал и потом уже подставляется в TextBlock)
        string _content;

        // Это словарь для правил подсветки и соответствующих им очередей задач
        Dictionary<HighlightRule, TaskQueue> _ruleTasks;

        /// <summary>
        /// Коллекция правил подсветки
        /// </summary>
        public HighlightRulesCollection HighlightRules
        {
            get
            {
                return (HighlightRulesCollection)GetValue(HighlightRulesProperty);
            }
            set
            {
                SetValue(HighlightRulesProperty, value);
            }
        }

        public static readonly DependencyProperty HighlightRulesProperty =
            DependencyProperty.Register("HighlightRules", typeof(HighlightRulesCollection), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(null) { PropertyChangedCallback = HighlightRulesChanged });


        static void HighlightRulesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var col = e.NewValue as HighlightRulesCollection;
            var tb = sender as HighlightTextBlock;
            if (col != null && tb != null)
            {
                col.CollectionChanged += tb.HighlightRules_CollectionChanged;
                foreach (var rule in col)
                {
                    rule.HighlightTextChanged += tb.Rule_HighlightTextChanged;
                }
            }
        }

        public HighlightTextBlock()
        {
            _ruleTasks = new Dictionary<HighlightRule, TaskQueue>();
            HighlightRules = new HighlightRulesCollection();
            InitializeComponent();
        }

        // Обработчик события на изменение коллекции правил подсветки
        void HighlightRules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (HighlightRule rule in e.NewItems)
                    {
                        _ruleTasks.Add(rule, new TaskQueue(1));
                        SubscribeRuleNotifies(rule);
                        BeginHighlight(rule);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (HighlightRule rule in e.OldItems)
                    {
                        //rule.HightlightedText = string.Empty;
                        _ruleTasks.Remove(rule);
                        UnsubscribeRuleNotifies(rule);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (HighlightRule rule in e.OldItems)
                    {
                        //rule.HightlightedText = string.Empty;
                        _ruleTasks.Remove(rule);
                        UnsubscribeRuleNotifies(rule);
                    }
                    break;
            }
        }

        // Подписка на события правила подсветки
        void SubscribeRuleNotifies(HighlightRule rule)
        {
            rule.HighlightTextChanged += Rule_HighlightTextChanged;
            rule.IgnoreCaseChanged += Rule_IgnoreCaseChanged;
        }

        void Rule_IgnoreCaseChanged(object sender, IgnoreCaseChangedEventArgs e)
        {
            BeginHighlight((HighlightRule)sender);
        }

        // Отписка от событий правила подсветки
        void UnsubscribeRuleNotifies(HighlightRule rule)
        {
            rule.HighlightTextChanged -= Rule_HighlightTextChanged;
            rule.IgnoreCaseChanged -= Rule_IgnoreCaseChanged;
        }

        // Обработчик события, которое срабатывает, когда текст для подсветки изменился
        void Rule_HighlightTextChanged(object sender, HighlightTextChangedEventArgs e)
        {
            BeginHighlight((HighlightRule)sender);
        }

        // Здесь запускается механизм подсвечивания в созданном мною диспетчере задач.
        // Смысл в том, что если текст вводится/стирается слишком быстро,
        // предыдущая подсветка не успеет закончить работу, поэтому новая подсветка
        // добавляется в очередь. Если в очереди уже что то есть, то это удаляется из очереди
        // и вставляется новая задача. Для каждого правила очередь своя.
        void BeginHighlight(HighlightRule rule)
        {
            _ruleTasks[rule].Add(new Action(() => Highlight(rule)));
        }

        // Механизм подсветки
        void Highlight(HighlightRule rule)
        {
            // Если передали не существующее правило, покидаем процедуру
            if (rule == null)
                return;

            // Так как правила у нас задаются в Xaml коде, они будут принадлежать основному потоку, в котором крутится форма,
            // поэтому некоторые свойства можно достать/положить только таким образом
            ObservableCollection<Highlight> highlights = null;
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                highlights = rule.Highlights;
            }));

            // Даже если существует правило, но в нем не задано, чем подсвечивать, покидаем процедуру подсветки
            if (highlights.Count == 0)
                return;

            // Еще ряд условий для выхода из процедуры подсветки
            var exitFlag = false;
            exitFlag = exitFlag || string.IsNullOrWhiteSpace(_content);
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                exitFlag = exitFlag || Inlines.IsReadOnly || Inlines.Count == 0 ||
                HighlightRules == null || HighlightRules.Count == 0;
            }));

            if (exitFlag)
                return;

            // Создадим параграф. Все манипуляции будем проводить внутри него, потому что выделить что либо
            // непосредственно в TextBlock нельзя, если это выделение затрагивает несколько элементов
            var par = new Paragraph();

            // Парсим _content, в котором у нас сериализованный Span с оригинальным содержимым TextBlock'a.
            var parsedSp = (Span)XamlReader.Parse(_content);

            // Сам Span нам не нужен, поэтому сливаем все его содержимое в параграф
            par.Inlines.AddRange(parsedSp.Inlines.ToArray());

            // Обозначаем стартовую позицию (просто для удобства) и выдергиваем из TextBlock'a голый текст. 
            // Искать вхождения искомой строки будем именно в нем
            var firstPos = par.ContentStart;
            var curText = string.Empty;
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                curText = Text;
            }));

            // Выдергиваем из основного потока текст для подсветки
            var hlText = string.Empty;
            Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
            {
                hlText = rule.HightlightedText;
            }));

            // Если текст для подсветки не пустой и его длина не превышает длину текста, в котором ищем, 
            // то продолжим, иначе просто выведем в конце оригинал
            if (!string.IsNullOrEmpty(hlText) && hlText.Length <= curText.Length)
            {
                // Выдергиваем в основном потоке из правила свойство IgnoreCase.
                // Решил логику оставиьт в основном потоке, потому что нагрузка операции очень низкая
                // и не стоит моего пота :)
                var comparison = StringComparison.CurrentCulture;
                Application.Current.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    comparison = rule.IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
                }));

                // Формируем список индексов, откуда начинаются вхождения искомой строки в тексте
                var indexes = new List<int>();
                var ind = curText.IndexOf(hlText, comparison);
                while (ind > -1)
                {
                    indexes.Add(ind);
                    ind = curText.IndexOf(hlText, ind + hlText.Length, StringComparison.CurrentCultureIgnoreCase);
                }

                TextPointer lastEndPosition = null;
                // Проходим по всем индексам начала вхождения строки поиска в текст
                foreach (var index in indexes)
                {
                    // Эта переменная нужна была в моих соисканиях наилучшего места для начала поиска,
                    // ведь индекс положения в string не соответствует реальному положению TextPointer'a.
                    // Поиск продолжается, поэтому переменную я оставил.
                    var curIndex = index;

                    // Начинаем поиск с последней найденной позиции либо перемещаем TextPointer вперед 
                    // на значение, равное индексу вхождения подстроки в текст
                    var pstart = lastEndPosition ?? firstPos.GetPositionAtOffset(curIndex);

                    // startInd является длиной текста между начальным TextPointer и текущей точкой начала подсветки
                    var startInd = new TextRange(pstart, firstPos.GetInsertionPosition(LogicalDirection.Forward)).Text.Length;
                    
                    // В результате нам нужно, чтобы startInd был равен curIndex
                    while (startInd != curIndex)
                    {
                        // Если честно, мне неще не встречались случаи, когда я обгонял startInd обгонял curIndex, однако
                        // решил оставить продвижение назад на случай более оптимизированного алгоритма поиска
                        if (startInd < curIndex)
                        {
                            // Смещаем точку начала подсветки на разницу curIndex - startInd
                            var newpstart = pstart.GetPositionAtOffset(curIndex - startInd);

                            // Иногда TextPointer оказывается между \r и \n, в этом случае начало подсветки
                            // сдвигается вперед. Чтобы этого избежать, двигаем его в следующую позицию для вставки
                            //if (pstart.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd)
                            //    pstart = newpstart.GetNextInsertionPosition(LogicalDirection.Backward);

                            var len = new TextRange(pstart, newpstart).Text.Length;
                            startInd += len;
                            pstart = newpstart;
                        }
                        else
                        {
                            var newpstart = pstart.GetPositionAtOffset(curIndex - startInd);
                            var len = new TextRange(pstart, newpstart).Text.Length;
                            startInd -= len;
                            pstart = newpstart;
                        }
                    }

                    // Ищем конечную точку подсветки аналогичным способом, как для начальной
                    var pend = pstart.GetPositionAtOffset(hlText.Length);
                    var delta = new TextRange(pstart, pend).Text.Length;
                    while (delta != hlText.Length)
                    {
                        if (delta < hlText.Length)
                        {
                            var newpend = pend.GetPositionAtOffset(hlText.Length - delta);
                            var len = new TextRange(pend, newpend).Text.Length;
                            delta += len;
                            pend = newpend;
                        }
                        else
                        {
                            var newpend = pend.GetPositionAtOffset(hlText.Length - delta);
                            var len = new TextRange(pend, newpend).Text.Length;
                            delta -= len;
                            pend = newpend;
                        }
                    }

                    // К сожалению, предложенным способом не получается разделить Hyperlink.
                    // Скорее всего это придется делать вручную, но пока такой необходимости нет, 
                    // поэтому, если начальной или конечной частью подсветки мы режем гиперссылку,
                    // то просто сдвигаем эти позиции. В общем ссылка либо полностью попадает в подсветку,
                    // либо не попадает совсем
                    var sHyp = (pstart?.Parent as Inline)?.Parent as Hyperlink;
                    var eHyp = (pend?.Parent as Inline)?.Parent as Hyperlink;
                    if (sHyp != null)
                        pstart = pstart.GetNextContextPosition(LogicalDirection.Forward);

                    if (eHyp != null)
                        pend = pend.GetNextContextPosition(LogicalDirection.Backward);

                    // Ну а тут применяем к выделению подсветки.
                    if (pstart.GetOffsetToPosition(pend) > 0)
                    {
                        var sp = new Span(pstart, pend);
                        foreach (var hl in highlights)
                            hl.SetHighlight(sp);
                    }
                    lastEndPosition = pend;
                }
            }

            // Здесь сериализуем получившийся параграф и в основном потоке помещаем его содержимое в TextBlock
            var parStr = XamlWriter.Save(par);
            Application.Current.Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                Inlines.Clear();
                Inlines.AddRange(((Paragraph)XamlReader.Parse(parStr)).Inlines.ToArray());
            })).Wait();
        }

        void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            // Здесь дергаем наполнение TextBlock'a и сериализуем его в строку,
            // чтобы накатывать подсветку всегда на оригинал.
            // Это лучше вынести в отдельный поток, но пока и так сойдет.
            var sp = new Span();
            sp.Inlines.AddRange(Inlines.ToArray());
            var tr = new TextRange(sp.ContentStart, sp.ContentEnd);
            using (var stream = new MemoryStream())
            {
                tr.Save(stream, DataFormats.Xaml);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    _content = reader.ReadToEnd();
                }
            }
            Inlines.AddRange(sp.Inlines.ToArray());

            // Запускаем подсветку для всех правил
            foreach (var rule in HighlightRules)
            {
                _ruleTasks.Add(rule, new TaskQueue(1));
                SubscribeRuleNotifies(rule);
                BeginHighlight(rule);
            }  
        }
    }

}

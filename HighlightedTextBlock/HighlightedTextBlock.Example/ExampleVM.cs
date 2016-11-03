using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace HighlightedTextBlock.Example
{
    public class ExampleVm : INotifyPropertyChanged
    {
        string _hlText;
        bool _ignoreCase;

        public string HlText
        {
            get
            {
                return _hlText;
            }

            set
            {
                _hlText = value;
                OnPropertyChanged(nameof(HlText));
            }
        }

        public bool IgnoreCase
        {
            get
            {
                return _ignoreCase;
            }

            set
            {
                _ignoreCase = value;
                OnPropertyChanged(nameof(IgnoreCase));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace HighlightedTextBlock
{
    public class HighlightRulesCollection : INotifyCollectionChanged, IList, IList<HighlightRule>
    {
        ObservableCollection<HighlightRule> _items;

        public HighlightRulesCollection()
        {
            _items = new ObservableCollection<HighlightRule>();
            _items.CollectionChanged += CollectionChanged;
        }

        public HighlightRule this[int index]
        {
            get
            {
                return ((IList<HighlightRule>)_items)[index];
            }

            set
            {
                ((IList<HighlightRule>)_items)[index] = value;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return ((IList)_items)[index];
            }

            set
            {
                ((IList)_items)[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<HighlightRule>)_items).Count;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return ((IList)_items).IsFixedSize;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<HighlightRule>)_items).IsReadOnly;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return ((IList)_items).IsSynchronized;
            }
        }

        public object SyncRoot
        {
            get
            {
                return ((IList)_items).SyncRoot;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Add(object value)
        {
            return ((IList)_items).Add(value);
        }

        public void Add(HighlightRule item)
        {
            ((IList<HighlightRule>)_items).Add(item);
        }

        public void Clear()
        {
            ((IList<HighlightRule>)_items).Clear();
        }

        public bool Contains(object value)
        {
            return ((IList)_items).Contains(value);
        }

        public bool Contains(HighlightRule item)
        {
            return ((IList<HighlightRule>)_items).Contains(item);
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)_items).CopyTo(array, index);
        }

        public void CopyTo(HighlightRule[] array, int arrayIndex)
        {
            ((IList<HighlightRule>)_items).CopyTo(array, arrayIndex);
        }

        public IEnumerator<HighlightRule> GetEnumerator()
        {
            return ((IList<HighlightRule>)_items).GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return ((IList)_items).IndexOf(value);
        }

        public int IndexOf(HighlightRule item)
        {
            return ((IList<HighlightRule>)_items).IndexOf(item);
        }

        public void Insert(int index, object value)
        {
            ((IList)_items).Insert(index, value);
        }

        public void Insert(int index, HighlightRule item)
        {
            ((IList<HighlightRule>)_items).Insert(index, item);
        }

        public void Remove(object value)
        {
            ((IList)_items).Remove(value);
        }

        public bool Remove(HighlightRule item)
        {
            return ((IList<HighlightRule>)_items).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<HighlightRule>)_items).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<HighlightRule>)_items).GetEnumerator();
        }
    }
}

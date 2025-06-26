using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WpfApp.Model
{
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        private readonly Action<T>? _onItemAdded;
        private bool _suppressNotification;

        public BulkObservableCollection(Action<T>? onItemAdded = null)
        {
            _onItemAdded = onItemAdded;
        }

        public void ReplaceAll(IEnumerable<T> items)
        {
            try
            {
                _suppressNotification = true;
                base.ClearItems();

                foreach (var item in items)
                {
                    base.Add(item);
                }
            }
            finally
            {
                _suppressNotification = false;
                // Beobachte alle Items NACHDEM die Collection fertig befüllt ist
                if (_onItemAdded != null)
                {
                    foreach (var item in Items)
                        _onItemAdded(item);
                }
                RaiseReset();
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) return;

            try
            {
                _suppressNotification = true;
                foreach (var item in items)
                {
                    base.Add(item);
                    _onItemAdded?.Invoke(item);
                }
            }
            finally
            {
                _suppressNotification = false;
                RaiseReset();
            }
        }

        private void RaiseReset()
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnPropertyChanged(e);
        }
    }
}

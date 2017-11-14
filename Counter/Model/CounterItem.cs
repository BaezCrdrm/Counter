using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Counter.Model
{
    public class CounterItem : ObservableItem
    {
#region Propierties
        private DateTime _modDate;
        private int _count;

        public int ID { get; set; }
        public string Title { get; set; }
        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                OnPropertyChanged("Count");
            }
        }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate
        {
            get { return _modDate; }
            set
            {
                _modDate = value;
                OnPropertyChanged("ModificationDate");
            }
        }
        #endregion

#region Constructors
        public CounterItem(ObservableCollection<CounterItem> _items)
        {
            this.ID = GenerateId(_items);
            Init();
        }

        private void Init()
        {
            _count = 0;
            this.CreationDate = DateTime.Now;
            _modDate = this.CreationDate;
        }
#endregion

        private int GenerateId()
        {
            Random rand = new Random();
            int _rid = rand.Next(100, 1000000);
            return _rid;
        }

        public int GenerateId(ObservableCollection<CounterItem> _items)
        {
            int _rid = 0;
            do
            {
                _rid = GenerateId();
            } while (_items.Count<CounterItem>(p => p.ID == _rid) != 0);
            return _rid;
        }    
    }
}

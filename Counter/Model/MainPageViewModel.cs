using Counter.Logic;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Counter.Model
{
    public class MainPageViewModel
    {
        public ObservableCollection<CounterItem> CounterItemsCollection { get; set; }
        private DataManager dm;

        public MainPageViewModel()
        {
            dm = new DataManager();
        }

        public void GetData()
        {
            dm.GetDataAsync.Start();
            CounterItemsCollection = dm.GetDataAsync.Result;
        }

        public bool SaveData()
        {
            try
            {
                dm.SaveData(this.CounterItemsCollection);
                System.Diagnostics.Debug.Write("The data was correcty saved");
                return true;
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.Write("There was a proble while saving the data");
                return false;
            }
        }

        public void ChangeValue(string _btnName, int _id)
        {
            if (_btnName == "btnFwd")
                this.CounterItemsCollection.FirstOrDefault<CounterItem>(p => p.ID == _id).Count++;
            else if (_btnName == "btnBack")
                this.CounterItemsCollection.FirstOrDefault<CounterItem>(p => p.ID == _id).Count--;

            this.CounterItemsCollection.FirstOrDefault<CounterItem>(p => p.ID == _id).ModificationDate = DateTime.Now;

            SaveData();
        }

        public void ReplaceItem(CounterItem _item)
        {
            _item.ModificationDate = DateTime.Now;
            this.CounterItemsCollection[
                this.CounterItemsCollection.IndexOf(
                    this.CounterItemsCollection.FirstOrDefault<CounterItem>(p => p.ID == _item.ID))]
                    = _item;
        }

        public void DeleteItem(int _id)
        {
            this.CounterItemsCollection.Remove(
                this.CounterItemsCollection.FirstOrDefault<CounterItem>(p => p.ID == _id));
            SaveData();
        }
    }
}

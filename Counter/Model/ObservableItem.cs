using System.ComponentModel;

namespace Counter.Model
{
    public class ObservableItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Private method for the PropertyChanged interface
        /// </summary>
        protected void OnPropertyChanged(string _name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(_name));
        }
    }
}

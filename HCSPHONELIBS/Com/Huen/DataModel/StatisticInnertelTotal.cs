using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Com.Huen.DataModel
{
    public class StatisticInnertelTotal : INotifyPropertyChanged
    {
        private int _total = 0;
        private int _busy = 0;
        private int _wait = 0;

        public int total
        {
            get
            {
                return _total;
            }
            set
            {
                _total = value;
                this.OnPropertyChanged("total");
            }
        }

        public int busy
        {
            get
            {
                return _busy;
            }
            set
            {
                _busy = value;
                this.OnPropertyChanged("busy");
            }
        }

        public int wait
        {
            get
            {
                return _wait;
            }
            set
            {
                _wait = value;
                this.OnPropertyChanged("wait");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

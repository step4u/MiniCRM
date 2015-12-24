using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.Huen.Libs;
using System.Windows;
using System.ComponentModel;

namespace Com.Huen.DataModel
{
    public class MinicrmButtonStates : INotifyPropertyChanged
    {
        private Style _RedialBtn = (Style)Application.Current.FindResource("btnRedial_off");
        private Style _TransferBtn = (Style)Application.Current.FindResource("btnCallPush_off");
        private Style _PullBtn = (Style)Application.Current.FindResource("btnCallPull_down");
        private Style _RecordBtn = (Style)Application.Current.FindResource("btnREC_off");

        public Style RedialBtn
        {
            get
            {
                return _RedialBtn;
            }
            set
            {
                _RedialBtn = value;
                this.OnPropertyChanged("RedialBtn");
            }
        }

        public Style TransferBtn
        {
            get
            {
                return _TransferBtn;
            }
            set
            {
                _TransferBtn = value;
                this.OnPropertyChanged("TransferBtn");
            }
        }

        public Style PullBtn
        {
            get
            {
                return _PullBtn;
            }
            set
            {
                _PullBtn = value;
                this.OnPropertyChanged("PullBtn");
            }
        }

        public Style RecordBtn
        {
            get
            {
                return _RecordBtn;
            }
            set
            {
                _RecordBtn = value;
                this.OnPropertyChanged("RecordBtn");
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

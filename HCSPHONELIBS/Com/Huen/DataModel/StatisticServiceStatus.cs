using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

namespace Com.Huen.DataModel
{
    public class StatisticServiceStatus : INotifyPropertyChanged
    {
        private string _value = string.Empty;
        public string Label { get; set; }

        public short Idx = 0;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                this.OnPropertyChanged("Value");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ServiceStatusList
    {
        public List<StatisticServiceStatus> Items;
        public ServiceStatusList()
        {
            Items = new List<StatisticServiceStatus>();

            this.Items.Add(new StatisticServiceStatus() { Idx = 0, Label = "현재시간", Value = DateTime.Now.ToString("yyyy-MM-dd HH시 mm분 ss초") });
            this.Items.Add(new StatisticServiceStatus() { Idx = 1, Label = "콜서비스 상태", Value = "시작" });
            this.Items.Add(new StatisticServiceStatus() { Idx = 2, Label = "콜서비스 업무시간", Value = "09:00 ~ 18:00" });
            this.Items.Add(new StatisticServiceStatus() { Idx = 3, Label = "전체 상담원 수", Value = "60" });
            this.Items.Add(new StatisticServiceStatus() { Idx = 4, Label = "녹취 서버 남은 용량", Value = "0" });
        }
    }
}

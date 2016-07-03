using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace CloudTechIot1dot5
{
    //http://www.cnblogs.com/cloudtech
    //cloudtechesx@gmail.com
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _currentTime;

        public string CurrentTime
        {
            get
            {
                return _currentTime;
            }

            set
            {
                _currentTime = value;
                OnProperityChanged("CurrentTime");
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void OnProperityChanged(string propertyName)
        {
            PropertyChanged?.Invoke(propertyName, new PropertyChangedEventArgs(propertyName));
        }
    }
}

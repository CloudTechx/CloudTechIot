using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Controls;

namespace CloudTechIot6
{
    //http://www.cnblogs.com/cloudtech
    //cloudtechesx@gmail.com
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Fileds
        //GPIO控制器
        //Gpio Controller
        private GpioController _gpioController;
        //引脚集合
        //Pin Collection
        private GpioPin[] _pins;
        //键码表
        //KeyCode Table
        private Dictionary<byte, char> _keyMaps;
        private bool _initCompleted;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        private string _msg;
        //键码
        //Key Code
        public string Msg
        {
            get
            {
                return _msg;
            }

            set
            {
                _msg = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("Msg"));
            }
        }

        public string FreshTime
        {
            get
            {
                return _freshTime;
            }

            set
            {
                _freshTime = value;
                OnPropertyChanged(this, new PropertyChangedEventArgs("FreshTime"));
            }
        }

        private string _freshTime;

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            _initCompleted = false;
            _keyMaps = new Dictionary<byte, char>();
            InitKeyMaps();
            //获取默认GPIO控制器
            //Get Default Gpio Controller
            _gpioController = GpioController.GetDefault();
            if (null == _gpioController)
            {
                throw new Exception("GpioController init failed");
            }
            //初始化 GPIO Pin
            //Init GPIO 引脚
            _pins = new GpioPin[] { _gpioController.OpenPin(5), _gpioController.OpenPin(6), _gpioController.OpenPin(13), _gpioController.OpenPin(19), _gpioController.OpenPin(12), _gpioController.OpenPin(16), _gpioController.OpenPin(20), _gpioController.OpenPin(21) };

            for (int i = 0; i < 8; i++)
            {
                //设置为输入并监听引脚电平变化
                //set input mode and listen pin level change
                if (i < 4)
                {
                    _pins[i].SetDriveMode(GpioPinDriveMode.Input);
                    _pins[i].DebounceTimeout = TimeSpan.FromMilliseconds(20);
                    _pins[i].ValueChanged += (GpioPin sender, GpioPinValueChangedEventArgs args) =>
                    {
                        lock (this)
                            //高电平
                            //high level 
                            if (_initCompleted && GpioPinEdge.RisingEdge == args.Edge)
                                //扫描列线
                                //scan column pin
                                for (int j = 4; j < 8; j++)
                                {
                                    _pins[j].Write(GpioPinValue.Low);
                                    if (GpioPinValue.Low == sender.Read())
                                    {
                                        //获取生成键码并输出到界面
                                        //generate keycode and print on UI
                                        Msg = _keyMaps[(byte)((1 << ToIndex(sender)) | (1 << j))].ToString();
                                        FreshTime = DateTime.Now.ToString("HH:mm:ss");
                                        _pins[j].Write(GpioPinValue.High);
                                        break;
                                    }
                                    _pins[j].Write(GpioPinValue.High);
                                }
                    };
                }
                //设置为输出高电平
                //set output high level
                else
                {
                    _pins[i].SetDriveMode(GpioPinDriveMode.Output);
                    _pins[i].Write(GpioPinValue.High);
                }
            }
            Msg = "Push Button";
            //初始化完成
            //initialize completed
            _initCompleted = true;
        }

        #endregion

        #region Methods

        //MVVM依赖属性通知事件
        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { PropertyChanged?.Invoke(sender, e); });
        }

        //初始化键码表
        //initialize keycode
        private void InitKeyMaps()
        {
            _keyMaps.Add(0x88, '1');
            _keyMaps.Add(0x84, '2');
            _keyMaps.Add(0x82, '3');
            _keyMaps.Add(0x81, 'A');
            _keyMaps.Add(0x48, '4');
            _keyMaps.Add(0x44, '5');
            _keyMaps.Add(0x42, '6');
            _keyMaps.Add(0x41, 'B');
            _keyMaps.Add(0x28, '7');
            _keyMaps.Add(0x24, '8');
            _keyMaps.Add(0x22, '9');
            _keyMaps.Add(0x21, 'C');
            _keyMaps.Add(0x18, '*');
            _keyMaps.Add(0x14, '0');
            _keyMaps.Add(0x12, '#');
            _keyMaps.Add(0x11, 'D');
        }

        //获取行线索引
        //get row pin index
        private int ToIndex(GpioPin pin)
        {
            int result = -1;
            for (int i = 0; i < _pins.Length; i++)
            {
                if (pin.Equals(_pins[i]))
                {
                    result = i;
                    break;
                }
            }
            if (0 > result)
            {
                throw new Exception("Unknow Pin Index");
            }
            else
            {
                return result;
            }
        }

        #endregion
    }
}

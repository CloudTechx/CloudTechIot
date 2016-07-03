using System;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;

namespace CloudTechIot2
{
    public sealed class StartupTask : IBackgroundTask
    {
        //http://www.cnblogs.com/cloudtech
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            //初始化Pins
            var pins = new LED[] { new LED(Colors.Red, 10), new LED(Colors.Yellow, 3), new LED(Colors.Green, 10) };
            //循环
            while (true)
            {
                //依次操作LED
                foreach (var pin in pins)
                {
                    pin.Open();
                    SendMsg(pin.Color.ToString());
                    //持续时间秒
                    Task.Delay(TimeSpan.FromSeconds(pin.Interval)).Wait();
                    pin.Close();
                }
            }
        }

        //输出结果
        private void SendMsg(string res)
        {
            //打印
            Debug.WriteLine(res);
            //推送到服务器
            HttpClient httpClient = new HttpClient();
            httpClient.GetAsync(new Uri(string.Format("http://192.168.1.5:8099/{0}", res)));
        }
    }
    //LED颜色
    public enum Colors : int
    {
        Red = 21,
        Yellow = 25,
        Green = 20
    }

    //LED控制类
    public sealed class LED
    {
        //GPIO
        private static GpioController _gpio;
        private Colors _color;
        //颜色
        public Colors Color
        {
            get
            {
                return _color;
            }
        }
        private GpioPin _pin;
        //Pin
        public GpioPin Pin
        {
            get
            {
                return _pin;
            }
        }
        private ushort _interval;
        //持续时间
        public ushort Interval
        {
            get
            {
                return _interval;
            }
        }
        //创建Pin
        public LED(Colors led, ushort interval)
        {
            _color = led;
            _pin = _gpio.OpenPin((int)_color);
            _pin.SetDriveMode(GpioPinDriveMode.Output);
            _interval = interval;
        }
        //初始化GPIO
        static LED()
        {
            _gpio = GpioController.GetDefault();
            if (null == _gpio)
            {
                throw new Exception("GPIO initial failed");
            }
        }
        //打开LED
        public void Open()
        {
            _pin.Write(GpioPinValue.Low);
        }
        //关闭LED
        public void Close()
        {
            _pin.Write(GpioPinValue.High);
        }
    }
}

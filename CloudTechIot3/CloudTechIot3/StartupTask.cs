using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace CloudTechIot3
{
    //http://www.cnblogs.com/cloudtech
    //cloudtechesx@gmail.com
    public sealed class StartupTask : IBackgroundTask
    {
        #region Fileds

        private DataReader _dataReader;
        private SerialDevice _derialPort;
        //缓冲区大小
        private uint _readBufferLength = 10;

        #endregion

        #region Main Method

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            await Listen();
            Close();
        }

        #endregion

        #region Private Methods

        //监听串口
        private async Task Listen()
        {
            try
            {
                //string aqsFilter = SerialDevice.GetDeviceSelector("UART0");
                //DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqsFilter);
                string aqs = SerialDevice.GetDeviceSelector("COM#");
                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqs);

                for (int i = 0; i < dis.Count; i++)
                {
                    string id = dis[i].Id;
                    var ss = dis[i];
                }
                //获取串口设备
                _derialPort = await SerialDevice.FromIdAsync(dis[0].Id);
                //串口设备是否获取成功
                if (null != _derialPort)
                {
                    _derialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);//超时
                    _derialPort.BaudRate = 9600;//波特率
                    _derialPort.Parity = SerialParity.None;//校验检查
                    _derialPort.StopBits = SerialStopBitCount.One;//停止位
                    _derialPort.DataBits = 8;//数据位
                    _derialPort.Handshake = SerialHandshake.None;//握手方式
                    //设置读取输入流
                    _dataReader = new DataReader(_derialPort.InputStream);
                    //循环读取数据
                    while (true)
                    {
                        await ReadAsync();
                    }
                }
                else
                {
                    //TODO
                }
            }
            catch (Exception ex)
            {
                //TODO
            }
            finally
            {
                Close();
            }
        }

        //异步读取数据
        private async Task ReadAsync()
        {
            Task<UInt32> loadAsyncTask;
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            //读取数据
            loadAsyncTask = _dataReader.LoadAsync(_readBufferLength).AsTask();
            Task.Delay(TimeSpan.FromSeconds(2.1)).Wait();
            uint bytesRead = await loadAsyncTask;
            //判断获取数据长度
            if (bytesRead > 0)
            {
                //转换十六进制数据
                string res = LoadData(bytesRead);
                SendMsg(res);
            }
            else
            {
                //TODO
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

        //转换数据
        private string LoadData(uint bytesRead)
        {
            StringBuilder str_builder = new StringBuilder();
            //转换缓冲区数据为16进制
            while (_dataReader.UnconsumedBufferLength > 0)
            {
                str_builder.Append(_dataReader.ReadByte().ToString("x2"));
            }
            return str_builder.ToString().ToUpper();
        }

        //释放资源
        private void Close()
        {
            if (null != _dataReader)
            {
                _dataReader.DetachStream();
            }
            if (null != _derialPort)
            {
                _derialPort.Dispose();
            }
        }

        #endregion
    }
}

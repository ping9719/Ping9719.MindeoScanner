using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ping9719.MindeoScanner
{
    /// <summary>
    /// 民德扫码器（支持一维码，二维码，主机模式，等...）
    /// </summary>
    public class ScannerCode
    {
        /// <summary>
        /// 串口
        /// </summary>
        public SerialPort SerialPort { get; private set; } = null;
        bool isHostRead = false;

        byte[] stateCode = new byte[] { 0x16, 0x54, 0x0D };//开始扫描
        byte[] endCode = new byte[] { 0x16, 0x55, 0x0D };//取消扫描

        /// <summary>
        /// 扫描到的所有消息事件数据
        /// </summary>
        /// <param name="sender">源：ScannerCode</param>
        /// <param name="mess">扫描到的消息文本</param>
        public delegate void ScanMessEventHandler(object sender, string mess);
        /// <summary>
        /// 扫描到的所有消息事件
        /// </summary>
        public event ScanMessEventHandler ScanMess;

        /// <summary>
        /// 打开扫码器
        /// </summary>
        /// <param name="portName">端口名</param>
        public void Open(string portName)
        {
            Open(portName, 9600, Parity.None, 8, StopBits.One);
        }

        /// <summary>
        /// 打开扫码器
        /// </summary>
        /// <param name="portName">端口名</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">奇偶校验位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        public void Open(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            if (SerialPort != null)
                throw new Exception("已经有打开的串口");

            SerialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            SerialPort.ReadTimeout = 500;
            SerialPort.ReadTimeout = 500;
            SerialPort.DataReceived += SerialPort_DataReceived;
            SerialPort.Open();
        }

        /// <summary>
        /// 关闭扫码器
        /// </summary>
        public void Close()
        {
            SerialPort?.Close();
            SerialPort = null;
        }

        string mess = string.Empty;//主机模式提供消息
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!isHostRead && ScanMess == null)
                return;

            mess = string.Empty;

            //直到缓存没有数据增加
            int topNum = 0;
            while (true)
            {
                if (topNum == SerialPort.BytesToRead || topNum >= Int16.MaxValue)
                    break;

                topNum = SerialPort.BytesToRead;

                Thread.Sleep(100);
            }

            var mess2 = SerialPort.ReadExisting();
            //去掉尾部一个换行
            if (mess2.EndsWith(Environment.NewLine))
                mess2 = mess2.Remove(mess2.Length - Environment.NewLine.Length);

            if (ScanMess != null)
                ScanMess(this, mess2);
            if (isHostRead)
            {
                mess = mess2;
                isHostRead = false;
            }
        }

        /// <summary>
        /// 在主机模式下执行一次
        /// </summary>
        /// <param name="keepTime">保持时长（秒）</param>
        /// <returns></returns>
        public string ReadHostOne(int keepTime = 4)
        {
            if (SerialPort == null)
                return null;

            //清空字符,还原状态
            SerialPort.ReadExisting();
            mess = string.Empty;
            isHostRead = true;

            var t1 = Task.Run(() =>
            {
                SerialPort.Write(stateCode, 0, stateCode.Length);
                Thread.Sleep(1000 * keepTime);
            });
            var t2 = Task.Run(() =>
            {
                while (isHostRead)
                {
                    Thread.Sleep(50);
                }
            });

            Task.WaitAny(t1, t2);
            return mess;
        }

        bool readHostForEnd = false;
        /// <summary>
        /// 在主机模式下一直执行，直到扫描到物品
        /// </summary>
        /// <param name="keepTime">保持时长（秒）</param>
        /// <param name="gapTime">扫描间隔时长（毫秒）</param>
        /// <returns>扫描结果</returns>
        public string ReadHostFor(int keepTime = 4, int gapTime = 1000)
        {
            return ReadHostFor(CancellationToken.None, keepTime);
        }

        /// <summary>
        /// 在主机模式下一直执行，直到扫描到物品
        /// </summary>
        /// <param name="keepTime">保持时长（秒）</param>
        /// <param name="gapTime">扫描间隔时长（毫秒）</param>
        /// <returns>扫描结果</returns>
        public string ReadHostFor(CancellationToken ct, int keepTime = 4, int gapTime = 1000)
        {
            if (SerialPort == null)
                return null;

            //清空字符,还原状态
            SerialPort.ReadExisting();
            mess = string.Empty;
            isHostRead = true;
            readHostForEnd = false;

            var t1 = Task.Run(() =>
            {

                while (!readHostForEnd)
                {
                    if (ct != CancellationToken.None)
                    {
                        //取消任务检测
                        if (ct == null || ct.IsCancellationRequested)
                            break;
                    }

                    SerialPort.Write(stateCode, 0, stateCode.Length);
                    Thread.Sleep(1000 * keepTime + 1000);
                }
            });
            var t2 = Task.Run(() =>
            {
                while (isHostRead)
                {
                    if (ct != CancellationToken.None)
                    {
                        //取消任务检测
                        if (ct == null || ct.IsCancellationRequested)
                            break;
                    }

                    Thread.Sleep(50);
                }
            });

            Task.WaitAny(t1, t2);
            readHostForEnd = true;

            SerialPort.Write(endCode, 0, endCode.Length);
            return mess;
        }

        /// <summary>
        /// 给扫码枪发送指令
        /// </summary>
        /// <param name="info">发送的命令</param>
        public void Send(byte[] info)
        {
            SerialPort.Write(info, 0, info.Length);
        }

        /// <summary>
        /// 给扫码枪发送取消扫描指令
        /// </summary>
        public void SendCancel()
        {
            SerialPort.Write(endCode, 0, endCode.Length);
        }
    }
}

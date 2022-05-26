﻿using System;
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
        SerialPort serialPort = null;
        bool isHostRead = false;

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
        /// <param name="portName"></param>
        public void Open(string portName)
        {
            if (serialPort != null)
                throw new Exception("已经有打开的串口");

            serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            serialPort.ReadTimeout = 500;
            serialPort.ReadTimeout = 500;
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();
        }

        /// <summary>
        /// 关闭扫码器
        /// </summary>
        public void Close()
        {
            serialPort?.Close();
            serialPort = null;
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
                if (topNum == serialPort.BytesToRead || topNum >= Int16.MaxValue)
                    break;

                topNum = serialPort.BytesToRead;

                Thread.Sleep(100);
            }

            var mess2 = serialPort.ReadExisting();
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
            if (serialPort == null)
                return null;

            //清空字符,还原状态
            serialPort.ReadExisting();
            mess = string.Empty;
            isHostRead = true;

            var t1 = Task.Run(() =>
            {
                byte[] sendscancode = new byte[] { 0x16, 0x54, 0x0D };
                serialPort.Write(sendscancode, 0, sendscancode.Length);
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
            if (serialPort == null)
                return null;

            //清空字符,还原状态
            serialPort.ReadExisting();
            mess = string.Empty;
            isHostRead = true;
            readHostForEnd = false;

            var t1 = Task.Run(() =>
            {
                byte[] stateCode = new byte[] { 0x16, 0x54, 0x0D };
                while (!readHostForEnd)
                {
                    if (ct != CancellationToken.None)
                    {
                        //取消任务检测
                        if (ct == null || ct.IsCancellationRequested)
                            break;
                    }

                    serialPort.Write(stateCode, 0, stateCode.Length);
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

            //取消扫描
            byte[] endCode = new byte[] { 0x16, 0x55, 0x0D };
            serialPort.Write(endCode, 0, endCode.Length);
            return mess;
        }

    }
}
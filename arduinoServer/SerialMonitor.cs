﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace arduinoServer
{
    class SerialMonitor
    {
        public static void logIt(String format, params Object[] arg)
        {
            Trace.WriteLine(String.Format(format, arg));
        }
        public static void logIt(String s)
        {
            Trace.WriteLine(s);
        }

        private SerialPort mSerialPort;
        private EventWaitHandle mStopEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        private EventWaitHandle mHeartEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        public EventWaitHandle mDataEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        private Dictionary<int, bool> keys = new Dictionary<int, bool>();
        public int Index = 0;
        private String sCom;
        private MemoryStream ms = new MemoryStream();
        private bool bStatus = true;
        private bool bExit = false;

        public String VersionInfo="1.0.0";

        public override string ToString()
        {
            return sCom;
        }

        public String ComName
        {
            get
            {
                return sCom;
            }
        }
        public bool Status
        {
            get
            {
                return bStatus;
            }
        }

        public void ExitThread()
        {
            logIt("ExitThread ++");
            mStopEvent.Set();
            bExit = true;
        }

        public void CopyLabelKeys(Dictionary<int, bool> kks, int[] labelmap)
        {
            lock (keys)
            {
                foreach (var pair in keys)
                {
                    if (pair.Key < labelmap.Length)
                    {
                        kks.Add(labelmap[pair.Key]+Index*labelmap.Length, pair.Value);
                    }
                }
            }
        }


        public void CopyKeys(Dictionary<int, bool> kks)
        {
            lock (keys)
            {
                foreach (var pair in keys)
                {
                    kks.Add(pair.Key, pair.Value);
                }
            }
        }

        public bool SendData(string ss)
        {
            bool ret = false;
            lock (this)
            {
                logIt($"SendData {sCom} ++ {ss}");
                if (String.IsNullOrEmpty(ss)) return ret;
                try
                {

                    mSerialPort.Write(ss);
                    ret = true;                 
                }
                catch (Exception e)
                {
                    logIt(e.ToString());
                    bStatus = false;
                }
                logIt($"SendData {sCom} -- {ret}");
                Thread.Sleep(150);
            }
            return ret;
        }

        public void OpenThread()
        {
            Thread.Sleep(1000);
            Thread thread1 = new Thread(ReadThread);
            thread1.Start();
        }

        public bool Open(String serialPort, Boolean bCreateThead = true)
        {
            logIt($"Open++ {serialPort}   {bCreateThead}");
            Close();
            bool result = false;
            if (!string.IsNullOrEmpty(serialPort))
            {
                sCom = serialPort;
                try
                {
                    mSerialPort = new SerialPort(serialPort, 9600);
                    mSerialPort.Parity = Parity.None;
                    mSerialPort.StopBits = StopBits.One;
                    mSerialPort.DataBits = 8;
                    mSerialPort.Handshake = Handshake.None;
                    mSerialPort.RtsEnable = true;
                    mSerialPort.DtrEnable = true;
                    mSerialPort.ReadTimeout = 1000;
                    mSerialPort.WriteTimeout = 1000;
                    mSerialPort.Open();
                    mSerialPort.DiscardInBuffer();
                    mSerialPort.DiscardOutBuffer();
                    mSerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    result = mSerialPort.IsOpen;
                    Thread.Sleep(50);
                    bStatus = true;
                }
                catch (Exception e)
                {
                    logIt(e.Message);
                    logIt(e.StackTrace);
                    bStatus = false;
                }
                logIt($"serial port {sCom} on {result}");
            }

            if (result && bCreateThead)
            {
                mStopEvent.Set();
                Thread.Sleep(1000);
                mStopEvent.Reset();
                Thread thread1 = new Thread(ReadThread);
                thread1.Start();
                (new Thread(MonitorPort)).Start();
            }

            return result;

        }

        private  void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int l = sp.BytesToRead;
            byte[] data = new byte[l];
            l = sp.Read(data, 0, l);
            lock (mStopEvent)
            {
                ms.Write(data, 0, l);
            }
            mHeartEvent.Set();
        }


        public bool Close()
        {
            bool result = false;
            if (null != mSerialPort && mSerialPort.IsOpen)
            {
                mSerialPort.Close();
                mSerialPort = null;
                result = true;
            }
            logIt($"{sCom} serial port off  {result}");
            bStatus = false;
            return result;
        }


        private void MonitorPort()
        {
            logIt("MonitorPort++");
            Thread.Sleep(6000);//start self detect.
            int irtry = 1;
            while (!bExit)
            {
                if (mHeartEvent.WaitOne(5000))
                {
                    Thread.Sleep(100);
                }
                else
                {
                    if (null == mSerialPort || !mSerialPort.IsOpen)
                    {
                        Thread.Sleep(irtry * 500);
                        bStatus = true;
                        Open(sCom);
                    }
                    if (null != mSerialPort && mSerialPort.IsOpen)
                    {
                        logIt($"{sCom} serial port open successfully.");
                        irtry = 1;
                    }
                    else
                    {
                        logIt($"{sCom} serial port open failed.");
                        irtry++;
                        if (irtry == 12) irtry = 12;
                    }
                }
            }
            logIt($"MonitorPort-- {bExit}");
        }

        public  void ReadThread()
        {
            String smsg = "";
            int iRetry = 0;
            logIt("ReadThread++");
           List<byte> llb = new List<byte>();
           while (!mStopEvent.WaitOne(10))
           {
                try
                {
                    lock (mStopEvent)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        int count = 0;
                        byte dd = 0;

                        while (count < ms.Length)
                        {
                            count++;
                            dd = (byte)ms.ReadByte();
                            if (dd == '\r' || dd == '\n')
                            {
                                if (llb.Count > 0)
                                {
                                    string message =Encoding.UTF8.GetString(llb.ToArray());// Console.WriteLine(Encoding.UTF8.GetString(llb.ToArray()));
                                    llb.Clear();
                                    //logIt($"{Index}: {message}");
                                    if (message.StartsWith("version:"))
                                    {
                                        VersionInfo = message.Replace("version: ", "");
                                        continue;
                                    }
                                    if (/*message.Length==34 && message.StartsWith("I,") && */String.Compare(smsg, message, true) != 0)
                                    {
                                        logIt($"{Index}: {message}");
                                        smsg = message;
                                        string[] status = message.Split(',');
                                        if (status[0] == "I")
                                        {
                                            lock (keys)
                                            {
                                                for (int i = 1; i < status.Length; ++i)
                                                {
                                                    keys[i - 1] = status[i] == "1";
                                                }
                                                mDataEvent.Set();
                                            }
                                        }
                                    }
                                }
                                continue;
                            }
                            if (dd == 0)
                            {
                                llb.Clear();
                                continue;
                            }
                            llb.Add(dd);
                        }
                        ms.SetLength(0);
                        if (llb.Count() > 0)
                        {
                            ms.Write(llb.ToArray(), 0, llb.Count);
                            llb.Clear();
                        }
                    }
                }
                catch (Exception e) {
                    logIt(e.ToString());
                    iRetry ++;
                }

                if (iRetry > 10) break;
            }
            if (mStopEvent.WaitOne(5))
            {
               // mStopEvent.Reset();
                Close();
            }
        }
    }
}

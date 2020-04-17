using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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
        public EventWaitHandle mDataEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        private Dictionary<int, bool> keys = new Dictionary<int, bool>();
        public int Index = 0;

        public void ExitThread()
        {
            mStopEvent.Set();
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
            logIt($"SendData ++ {ss}");
            if (String.IsNullOrEmpty(ss)) return ret;
            try {
                
                mSerialPort.Write(ss);
                ret = true;
            }
            catch (Exception e)
            {

            }
            logIt($"SendData -- {ret}");
            return ret;
        }

        public bool Open(String serialPort)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(serialPort))
            {
                try
                {
                    mSerialPort = new SerialPort(serialPort, 19200);
                    mSerialPort.Parity = Parity.None;
                    mSerialPort.StopBits = StopBits.One;
                    mSerialPort.DataBits = 8;
                    mSerialPort.Handshake = Handshake.None;
                    mSerialPort.RtsEnable = true;
                    mSerialPort.DtrEnable = true;
                    mSerialPort.ReadTimeout = 1000;
                    mSerialPort.WriteTimeout = 1000;

                    mSerialPort.Open();
                    result = mSerialPort.IsOpen;

                }
                catch (Exception e)
                {
                    logIt(e.Message);
                    logIt(e.StackTrace);
                }
                logIt("serial port " + mSerialPort.PortName + " on " + result);
            }

            if (result)
            {
                Thread thread1 = new Thread(ReadThread);
                thread1.Start();
            }

            return result;

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
            logIt("serial port off " + result);
            return result;
        }

        public  void ReadThread()
        {
            String smsg = "";
            while (!mStopEvent.WaitOne(5))
            {
                try
                {
                    string message = mSerialPort.ReadLine();
                    if (String.Compare(smsg, message, true) != 0)
                    { 
                        logIt($"{Index}: {message}");
                        smsg = message;
                        string[] status = message.Split(',');
                        if (status[0] == "I")
                        {
                            lock (keys)
                            {
                                for(int i = 1; i < status.Length; ++i)
                                {
                                    keys[i - 1] = status[i] == "1";
                                }
                                mDataEvent.Set();
                            }
                        }
                    }
                }
                catch (TimeoutException) { }
            }
            Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using arduinoServer.Properties;
using System.Timers;
using System.Text.RegularExpressions;
using System.Management;

namespace arduinoServer
{
    class SerialMonitor
    {
        public static void logIt(String format, params Object[] arg)
        {
            Trace.WriteLine($"[arduinoServer]:{String.Format(format, arg)}");
        }
        public static void logIt(String s)
        {
            Trace.WriteLine($"[arduinoServer]:{s}");
        }
        static public Boolean bSerialChanged = false;
        private SerialPort mSerialPort;
        private CH340Lib.CH340 mCh340Port;
        private EventWaitHandle mStopEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        private EventWaitHandle mHeartEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        public EventWaitHandle mDataEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        private EventWaitHandle m_Ch340ErrorEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private Dictionary<int, bool> keys = new Dictionary<int, bool>();
        private Object _myLock = new Object();
        private Object _myLockRead = new Object();
        public int Index = 0;
        private String sCom;
        private String _locationpaths;
        private MemoryStream ms = new MemoryStream();
        private bool bStatus = true;
        private bool bExit = false;
        private System.Timers.Timer aTimer;
        public String VersionInfo = "1.0.0";

        private  Thread threadread = null;
        private  Thread threadmoniport = null;
        int nErrorCnt = 0;
        bool bErrorTooMuch = false;

        public override string ToString()
        {
            return sCom;
        }

        public String LocationPaths
        {
            set { _locationpaths = value; }
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
                        kks.Add(labelmap[pair.Key] + Index * labelmap.Length, pair.Value);
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
                    if (Properties.Settings.Default.USEDLLCOMM)
                    {
                        int retlen = mCh340Port.WriteBytes(Encoding.ASCII.GetBytes(ss));
                        ret = retlen == ss.Length;
                    }
                    else
                    {
                        mSerialPort.Write(ss);
                    }
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

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            if (aTimer == null)
            {
                aTimer = new System.Timers.Timer(10000);
                // Hook up the Elapsed event for the timer. 
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
        }
        private void ResetTimer()
        {
            if (aTimer != null)
            {
                aTimer.Stop();
                aTimer.Start();
            }
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (mSerialPort != null)
            {
                logIt("Detect Fixture No response at {0:HH:mm:ss.fff}, open status: {1}", e.SignalTime, mSerialPort.IsOpen);
                Open(sCom, false, false);
            }
            else
            {
                logIt("mSerialPort is null, {0}", sCom);
            }
        }

        public bool Open(String serialPort, Boolean bCreateThead = true, Boolean bdtr = true)
        {
            logIt($"Open++ {serialPort}   {bCreateThead}");
            bool result = false;
            if (Properties.Settings.Default.USEDLLCOMM)
            {
                if (!string.IsNullOrEmpty(serialPort))
                {
                    sCom = serialPort;
                    if (mCh340Port != null)
                    {
                        mCh340Port.Close();
                    }
                    mCh340Port = new CH340Lib.CH340();
                    if (mCh340Port.Open(sCom))
                    {
                        Thread.Sleep(50);
                        bStatus = true;
                        result = true;
                        mCh340Port.onDataArrival += CH340_onDataArrival;
                        mCh340Port.onError += CH340_onError;

                    }
                    logIt($"serial port {sCom} on {result}");
                }
                if (result && bCreateThead)
                {
                    if (threadread == null)
                    {
                        threadread = new Thread(ReadThreadDll);
                        threadread.Name = $"ReadThread_{sCom}";
                        threadread.Start();
                    }

                    if (threadmoniport == null)
                    {
                        threadmoniport = new Thread(MonitorPortDLL);
                        threadmoniport.Name = $"Monitor_{sCom}";
                        threadmoniport.Start();
                    }

                }
            }
            else
            {
                SetTimer();
                Close();
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
                        mSerialPort.DtrEnable = bdtr;
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
                    if (threadread == null)
                    {
                        threadread = new Thread(ReadThread);
                        threadread.Name = $"ReadThread_{sCom}";
                        threadread.Start();
                    }
                    if (threadmoniport == null)
                    {
                        threadmoniport = new Thread(MonitorPort);
                        threadmoniport.Name = $"Monitor_{sCom}";
                        threadmoniport.Start();
                    }
                }
            }

            return result;

        }

        private void CH340_onDataArrival(int A_0)
        {
            m_Ch340ErrorEvent.Reset();
            nErrorCnt = 0;
            if (Settings.Default.IsPrintData)
            {
                logIt($"{sCom}: onDataArrival {A_0}");
            }
        }

        private void CH340_onError(int A_0)
        {
            m_Ch340ErrorEvent.Set();
            logIt($"{sCom} onError {A_0} cnt={nErrorCnt++}");
            if (nErrorCnt > 100)
            {
                bErrorTooMuch = true;
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int l = sp.BytesToRead;
            byte[] data = new byte[l];
            l = sp.Read(data, 0, l);
            lock (mStopEvent)
            {
                ms.Write(data, 0, l);
            }
            if (Settings.Default.IsPrintData)
            {
                logIt($"{sCom}: {Encoding.UTF8.GetString(data, 0, l)}");
            }
            ResetTimer();
            mHeartEvent.Set();
        }


        public bool Close()
        {
            bool result = false;
            if (Properties.Settings.Default.USEDLLCOMM)
            {
                if (mCh340Port != null)
                {
                    mCh340Port.Close();
                    result = true;
                }
            }
            else
            {
                if (null != mSerialPort && mSerialPort.IsOpen)
                {
                    mSerialPort.Close();
                    mSerialPort = null;
                    result = true;
                }
            }
            logIt($"{sCom} serial port off  {result}");
            bStatus = false;
            return result;
        }

        static public List<string> GetColorSensorPorts()
        {
            List<string> l = new List<string>();
            {
                Regex r = new Regex(@"^USB-SERIAL CH340 \(([COM\d]+)\)$");
                ManagementClass mc = new ManagementClass("Win32_PnPEntity");
                ManagementObjectCollection mcCollection = mc.GetInstances();
                foreach (ManagementObject mo in mcCollection)
                {
                    string s = mo["Description"]?.ToString();
                    if (string.Compare(s, "USB-SERIAL CH340") == 0)
                    {
                        //System.Diagnostics.Trace.WriteLine($"device: '{mo["Description"]}'");
                        String ss = mo["Caption"].ToString();
                        Match m = r.Match(ss);
                        if (m.Success)
                        {
                            l.Add(m.Groups[1].Value);
                        }
                        //l.Add(mo["Caption"].ToString());
                    }
                }
            }
            return l;
        }

        private void MonitorPortDLL()
        {
            logIt("MonitorPortDLL++");
            Thread.Sleep(10000);//start self detect.
            bErrorTooMuch = false;
            int irtry = 1;
            if (Monitor.TryEnter(_myLock))
            {
                try
                {
                    while (!bExit)
                    {
                        if (mHeartEvent.WaitOne(5000) && !bErrorTooMuch)
                        {
                            Thread.Sleep(100);
                        }
                        else
                        {
                            logIt("time out, May be Fixture Dead.");
                            if (null == mCh340Port || !mCh340Port.IsOpen()) 
                            {
                                Thread.Sleep(irtry * 500);
                                bStatus = true;
                                if (bSerialChanged)
                                {
                                    PortMapping portMapping = new PortMapping();
                                    //var ports = GetColorSensorPorts();
                                    var curports = portMapping.GetCh340Serial();
                                    if (curports.ContainsKey(_locationpaths))
                                    {
                                        logIt($"Change {sCom} to {curports[_locationpaths]}");
                                        sCom = curports[_locationpaths];
                                    }
                                }
                                Open(sCom, false, false);
                            }
                            if (null != mCh340Port && mCh340Port.IsOpen())
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
                }
                finally
                {
                    Monitor.Exit(_myLock);
                }
            }
            else
            {
                logIt("Thread MonitorPortDLL has running.");
            }

            logIt($"MonitorPortDLL-- {bExit}");
        }



        private void MonitorPort()
        {
            logIt("MonitorPort++");
            Thread.Sleep(10000);//start self detect.
            int irtry = 1;
            if (Monitor.TryEnter(_myLock))
            {
                try
                {
                    while (!bExit)
                    {
                        if (mHeartEvent.WaitOne(5000))
                        {
                            Thread.Sleep(100);
                        }
                        else
                        {
                            logIt("time out, May be Fixture Dead.");
                            if (null == mSerialPort || !mSerialPort.IsOpen)
                            {
                                Thread.Sleep(irtry * 500);
                                bStatus = true;
                                if (bSerialChanged)
                                {
                                    PortMapping portMapping = new PortMapping();
                                    //var ports = GetColorSensorPorts();
                                    var curports = portMapping.GetCh340Serial();
                                    if (curports.ContainsKey(_locationpaths))
                                    {
                                        logIt($"Change {sCom} to {curports[_locationpaths]}");
                                        sCom = curports[_locationpaths];
                                    }
                                }
                                Open(sCom, false, false);
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
                }
                finally
                {
                    Monitor.Exit(_myLock);
                }
            }
            else
            {
                logIt("Thread MonitorPort has running.");
            }

            
            logIt($"MonitorPort-- {bExit}");
        }

        private void ReadThreadDll()
        {
            logIt("ReadThreadDll ++");
            String smsg = "";
            while (!mStopEvent.WaitOne(40))
            {
                var lines = mCh340Port.ReadLines();
                foreach (var message in lines)
                {
                    mHeartEvent.Set();
                    if (message.StartsWith("version:"))
                    {
                        VersionInfo = message.Replace("version: ", "");
                        continue;
                    }
                    if (String.Compare(smsg, message, true) != 0)
                    {
                        logIt($"[{Thread.CurrentThread.ManagedThreadId}]{Index}: {message}");
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
                            }
                            mDataEvent.Set();
                        }
                    }
                }
            }
            logIt("ReadThreadDll --");
        }

        public void ReadThread()
        {
            String smsg = "";
            int iRetry = 0;
            logIt("ReadThread++");
            List<byte> llb = new List<byte>();
            List<string> messages = new List<string>();
            if (Monitor.TryEnter(_myLockRead))
            {
                try
                {
                    while (!mStopEvent.WaitOne(45))
                    {
                        try
                        {
                            lock (mStopEvent)
                            {
                                llb.Clear();
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
                                            string message = Encoding.UTF8.GetString(llb.ToArray());// Console.WriteLine(Encoding.UTF8.GetString(llb.ToArray()));
                                            llb.Clear();
                                            messages.Add(message);
                                            //logIt($"{Index}: {message}");
                                            //if (message.StartsWith("version:"))
                                            //{
                                            //    VersionInfo = message.Replace("version: ", "");
                                            //    continue;
                                            //}
                                            //if (String.Compare(smsg, message, true) != 0)
                                            //{
                                            //    logIt($"[{Thread.CurrentThread.ManagedThreadId}]{Index}: {message}");
                                            //    smsg = message;
                                            //    string[] status = message.Split(',');
                                            //    if (status[0] == "I")
                                            //    {
                                            //        lock (keys)
                                            //        {
                                            //            for (int i = 1; i < status.Length; ++i)
                                            //            {
                                            //                keys[i - 1] = status[i] == "1";
                                            //            }
                                            //        }
                                            //        mDataEvent.Set();
                                            //    }
                                            //}
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
                                    //if (Settings.Default.IsPrintData)
                                    //{
                                    //    Program.logIt($"copy to ms: len={llb.Count()}: str={Encoding.UTF8.GetString(llb.ToArray())}");
                                    //}
                                    ms.Write(llb.ToArray(), 0, llb.Count);
                                    //llb.Clear();
                                }
                            }

                            if (llb.Count() > 0 && Settings.Default.IsPrintData)
                            {
                                Program.logIt($"copy to ms: len={llb.Count()}: str={Encoding.UTF8.GetString(llb.ToArray())}");
                            }
                            llb.Clear();

                            foreach (var message in messages)
                            {
                                if (message.StartsWith("version:"))
                                {
                                    VersionInfo = message.Replace("version: ", "");
                                    continue;
                                }
                                if (String.Compare(smsg, message, true) != 0)
                                {
                                    logIt($"[{Thread.CurrentThread.ManagedThreadId}]{Index}: {message}");
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
                                        }
                                        mDataEvent.Set();
                                    }
                                }
                            }
                            messages.Clear();
                        }
                        catch (Exception e)
                        {
                            logIt(e.ToString());
                            iRetry++;
                        }

                        if (iRetry > 10) break;
                    }
                }
                finally
                {
                    Monitor.Exit(_myLockRead);
                }
            }
            else
            {
                logIt("Thread ReadDataThread has running.");
                return;
            }
            
            if (mStopEvent.WaitOne(5))
            {
                // mStopEvent.Reset();
                Close();
            }
            logIt("ReadThread--");
        }
    }
}

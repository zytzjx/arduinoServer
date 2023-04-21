using arduinoServer.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace arduinoServer
{
    struct RGB
    {
        public byte r;
        public byte g;
        public byte b;
        public RGB(byte rr, byte gg, byte bb)
        {
            r = rr;
            g = gg;
            b = bb;
        }
        public override string ToString() => $"{r},{g},{b},";
    };

    class SerialManager
    {
        public Dictionary<int, bool> buttonstatus = new Dictionary<int, bool>();
        private List<SerialMonitor> serials = new List<SerialMonitor>();
        private List<EventWaitHandle> waitHandles = new List<EventWaitHandle>();
        //private Dictionary<String, Object> config = new Dictionary<string, object>();
        private Config config = new Config();
        private EventWaitHandle mStopEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        //private int GroupCnt = 0;
        private int MAX_Groupt = 0;
        private int CONFIG_MAX_LABEL = 0;
        RGB[] status_leds;

        private bool FinishInit = false;

        private Dictionary<int, TcpClient> callbacklist = new Dictionary<int, TcpClient>();

        ////private int oldStripSelect = 0;
        //public List<string> GetColorSensorPorts()
        //{
        //    List<string> l = new List<string>();
        //    {
        //        Regex r = new Regex(@"^USB-SERIAL CH340 \(([COM\d]+)\)$");
        //        ManagementClass mc = new ManagementClass("Win32_PnPEntity");
        //        ManagementObjectCollection mcCollection = mc.GetInstances();
        //        foreach (ManagementObject mo in mcCollection)
        //        {
        //            string s = mo["Description"]?.ToString();
        //            if (string.Compare(s, "USB-SERIAL CH340") == 0)
        //            {
        //                //System.Diagnostics.Trace.WriteLine($"device: '{mo["Description"]}'");
        //                String ss = mo["Caption"].ToString();
        //                Match m = r.Match(ss);
        //                if (m.Success)
        //                {
        //                    l.Add(m.Groups[1].Value);
        //                }
        //                //l.Add(mo["Caption"].ToString());
        //            }
        //        }
        //    }
        //    return l;
        //}

        public void SendDatatoCallBack(String message)
        {
            if (!String.IsNullOrEmpty(message))
            {
                Program.logIt($"Send Message to Server: {message}");
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message+ Environment.NewLine);
                lock (callbacklist) {
                    List<int> keys = new List<int>(callbacklist.Keys);
                    foreach (int key in keys)
                    {
                        try
                        {
                            TcpClient tcpc = callbacklist[key];
                            if (!tcpc.Connected)
                            {
                                tcpc.Close();
                                if (arduinoServer.Properties.Settings.Default.CLIENTTRYCONN)
                                {// this long time
                                    TcpClient tcptemp = new TcpClient("localhost", key);
                                    callbacklist[key] = tcptemp;
                                    tcptemp.GetStream().Write(data, 0, data.Length);
                                }
                                else
                                {
                                    //this fast
                                    callbacklist.Remove(key);
                                }
                            }
                            else
                            {
                                tcpc.GetStream().Write(data, 0, data.Length);
                            }
                        }
                        catch(Exception e)
                        {
                            Program.logIt(e.ToString());
                        }
                    }
                }
            }
        }

        public Boolean RemoveCallBackList(int Port)
        {
            Boolean bret = true;
            if (callbacklist.ContainsKey(Port))
            {
                lock (callbacklist)
                {
                    TcpClient tcp = callbacklist[Port];
                    callbacklist.Remove(Port);
                    bret = true;
                    try
                    {
                        tcp.Close();
                    }
                    catch (Exception e)
                    {
                        Program.logIt(e.ToString());
                    }

                }
            }
            return bret;
        }

        public List<int> GetCallBackPort()
        {
            lock (callbacklist)
            {
                return callbacklist.Keys.ToList();
            }
        }

        public Boolean AddCallBackList(int Port)
        {
            Boolean bret = false;
            lock (callbacklist)
            {
                if (!callbacklist.ContainsKey(Port))
                {
                    try
                    {
                        TcpClient tcp = new TcpClient("localhost", Port);
                        callbacklist[Port] = tcp;
                        bret = true;
                    }
                    catch (Exception e)
                    {
                        Program.logIt(e.ToString());
                        bret = false;
                    }

                }
            }
            return bret;
        }

        public String SerialCountInfo()
        {
            var infos = new Dictionary<String, Object>();
            infos["count"] = MAX_Groupt;
            List<String> sSerial = new List<string>();
            foreach (var ss in serials)
            {
                sSerial.Add(ss.ToString());
            }
            infos["serails"] = sSerial;
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(infos);
        }

        public void Init()
        {
            config.LoadConfigFile(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Serialconfig.json"));

            List<string> sComsPC = SerialMonitor.GetColorSensorPorts();
            Program.logIt($"System Exists include:{String.Join(", ", sComsPC.ToArray())}");

            List<string> sComsConfig = config.GetUseCommList(sComsPC);
            Program.logIt($"Config file include:{String.Join(", ", sComsConfig.ToArray())}");

            List<String> sComs = sComsConfig.Intersect(sComsPC).ToList();
            Program.logIt($"serial coms count {sComs.Count}");

            if (sComsConfig.Count == 1 && sComsPC.Count == 1)
            {
                Program.logIt($"using pc list serial port, because config 1 com.");
                sComs = sComsPC;
            }
            //if (config.ContainsKey("serialports"))
            //{
            //    if (((Object[])config["serialports"]).Length > 1)
            //    {
            //        sComs = ((Object[])config["serialports"]).Cast<String>().ToList();
            //    }
            //}
            
            int index = 0;
            for (int i = 0; i< sComs.Count; i++){
                String sComName = sComs[i];

                {
                    SerialMonitor sertmp = new SerialMonitor()
                    {
                        LocationPaths = config.FConfigs[index.ToString()].Serialindex["0"],
                        Index = index++
                    };


                    Program.logIt($"{sComName} opening");
                    int iretry = 5;
                    
                    while (!sertmp.Open(sComName))
                    {
                        Thread.Sleep(1000);
                        if (iretry-- < 0) break;
                    }
                    //if (!Settings.Default.LIGHTLED)
                    //{
                    //    sertmp.SendData("N\r");
                    //    sertmp.SendData("N\r");
                    //}
                    serials.Add(sertmp);
                    waitHandles.Add(sertmp.mDataEvent);
                }
            }

            MAX_Groupt = serials.Count();
            CONFIG_MAX_LABEL = config.LabelCount;//GroupCnt * MAX_Groupt
            status_leds = new RGB[CONFIG_MAX_LABEL];
            for(int i = 0; i < CONFIG_MAX_LABEL; i++)
            {
                status_leds[i] = new RGB(0, 0, 0);
            }

            for(int i = 1; i <= arduinoServer.Properties.Settings.Default.MAX_SUPPORT_LABEL; i++)
            {
                buttonstatus[i] = false;
            }

            Thread.Sleep(5000);
            //Cleanup();

            Thread thread1 = new Thread(MonitorThread);
            thread1.Start();

            EZUSB ezUSB = new EZUSB();

            ezUSB.AddUSBEventWatcher(USBEventHandler, USBEventHandler, new TimeSpan(0, 0, 1));

            FinishInit = true;
        }

        private void USBEventHandler(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent")
            {
                Program.logIt("USB plug in time:" + DateTime.Now);
            }
            else if (e.NewEvent.ClassPath.ClassName == "__InstanceDeletionEvent")
            {
                Program.logIt("USB plug out time:" + DateTime.Now);
            }
            foreach (EZUSB.USBControllerDevice Device in EZUSB.WhoUSBControllerDevice(e))
            {
                Program.logIt("\tAntecedent：" + Device.Antecedent);
                Program.logIt("\tDependent：" + Device.Dependent);
                if (Device.Dependent.Contains(@"USB\\VID_1A86&PID_7523"))
                {
                    SerialMonitor.bSerialChanged = true;
                }
               
            }
        }

        private void AddStatus(SerialMonitor sertmp)
        {
            Dictionary<int, bool> bstatus = new Dictionary<int, bool>();
            sertmp.CopyKeys(bstatus);
            int[] plabels = config.CurLabels(sertmp.Index).ToArray();///((Object[])config["portlabel"]).Cast<int>().ToArray();
            int nLabelStart = config.GetLessCurlabels(sertmp.Index);///sertmp.Index * plabels.Length;
            lock (buttonstatus)
            {
                for(int i = 0; i < plabels.Length; i++)
                {
                    if (bstatus.ContainsKey(i))
                    {
                        if ((buttonstatus[nLabelStart + plabels[i]] == true) && bstatus[i]==false)
                        {
                            SendDatatoCallBack($"release:{nLabelStart + plabels[i]}");
                        }
                        else if ((buttonstatus[nLabelStart + plabels[i]] == false) && bstatus[i] == true)
                        {
                            SendDatatoCallBack($"pressed:{nLabelStart + plabels[i]}");
                        }
                        buttonstatus[nLabelStart + plabels[i]] = bstatus[i];
                    }
                    else
                    {
                        Program.logIt("Config file has problem!!!!!!!");
                    }
                }
            }
        }

        void MonitorThread()
        {
            waitHandles.Add(mStopEvent);
            while (true) {
                int i = WaitHandle.WaitAny(waitHandles.ToArray());
                if (i == waitHandles.Count() - 1)
                {
                    break;
                }
                else
                {
                    SerialMonitor smonitor = serials[i];
                    AddStatus(smonitor);
                }
            }

            Uninit();
        }

        public Dictionary<int, string> HWVersion()
        {
            Dictionary<int, string> ret = new Dictionary<int, string>();
            int index = 0;
            foreach (var ser in serials)
            {
                try
                {
                    if (!ser.SendData("V\r"))
                    {
                        Program.logIt("Get Version failed");
                        ret[index++] = "";
                        continue;
                    }
                    Thread.Sleep(200);
                    ret[index++] = ser.VersionInfo;
                    //ret[index++] = "333";
                    //ret[index++] = "444";
                }
                catch (Exception)
                {
                    Program.logIt("cleanup failed exception");
                }

            }
            return ret;
        }

        public bool ledsOn()
        {
            bool bret = true;
            Thread.Sleep(500);
            for (int i = 0; i < 2; i++)
            {
                foreach (var ser in serials)
                {
                    try
                    {
                        if (!ser.SendData("L\r"))
                        {
                            bret = false;
                            Program.logIt("cleanup failed");
                        }
                    }
                    catch (Exception)
                    {
                        Program.logIt("cleanup failed exception");
                    }

                }
                Thread.Sleep(50);
            }
            for (int i = 0; i < CONFIG_MAX_LABEL; i++)
            {
                status_leds[i] = new RGB(255, 255, 255);
            }
            return bret;
        }

        public bool Cleanup()
        {
            Program.logIt("Cleanup ++");
            bool bret = true;
            Thread.Sleep(500);
            for (int i = 0; i < 2; i++)
            {
                foreach (var ser in serials)
                {
                    try
                    {
                        if (!ser.SendData("C\r"))
                        {
                            bret = false;
                            Program.logIt("cleanup failed");
                        }
                    }
                    catch (Exception)
                    {
                        Program.logIt("cleanup failed exception");
                    }

                }
                Thread.Sleep(50);
            }
            for (int i = 0; i < CONFIG_MAX_LABEL; i++)
            {
                status_leds[i] = new RGB(0, 0, 0);
            }
            return bret;
        }

        public Dictionary<String, bool> GetStatus()
        {
            Dictionary<String, bool> ret = new Dictionary<string, bool>();
            
            foreach (var ser in serials)
            {
                try
                {
                    if (FinishInit) {
                        ret[ser.ComName] = ser.Status;
                    }
                    else
                    {
                        ret[ser.ComName] = false;
                    }
                    
                }
                catch (Exception)
                {

                }
               
            }
            return ret;
        }

        public Dictionary<int, bool> GetKey(int id)
        {
            Dictionary<int, bool> ret = new Dictionary<int, bool>();
            foreach (var ser in serials)
            {
                //if (ser.Index * GroupCnt <= id && id < (ser.Index + 1))
                if (config.GetLessCurlabels(ser.Index) <= id && id < config.GetLessCurlabels(ser.Index+1))
                {
                    Dictionary<int, bool> temp = new Dictionary<int, bool>();
                    ser.CopyLabelKeys(ret, config.CurLabels(ser.Index).ToArray());///((Object[])config["portlabel"]).Cast<int>().ToArray());
                    break;
                }
            }

            return ret;
        }


        public Dictionary<int, Boolean> GetKeys()
        {
            Dictionary<int, Boolean> ret = new Dictionary<int, Boolean>();
            foreach (var ser in serials)
            {
                Dictionary<int, bool> temp = new Dictionary<int, bool>();
                ser.CopyLabelKeys(temp, config.CurLabels(ser.Index).ToArray());///((Object[])config["portlabel"]).Cast<int>().ToArray());
                foreach (var kk in temp)
                {
                    ret[kk.Key] = kk.Value;
                }
            }

            return ret;
        }

        public Dictionary<string, bool> SendData(String s)
        {
            var serializer = new JavaScriptSerializer();
            var sendLed = (Dictionary<String, Object>)serializer.DeserializeObject(s);
            bool bret = false;
            if (String.Compare(sendLed["status"].ToString(),"status", true) == 0)
            {
               bret= SendStatusLED(sendLed);
            }
            else if (String.Compare(sendLed["status"].ToString(), "test", true) == 0)
            {
                bret = SendTestLed(sendLed);
            }
            else 
            {
                bret = SendStrip(sendLed);
            }

            Dictionary<string, bool> sss = new Dictionary<string, bool>();
            sss["result"] = bret;

            return sss;
        }

        bool SendTestLed(Dictionary<String, Object> inpt)
        {
            bool bret = false;
            int label = Convert.ToInt32(inpt["label"]) - 1;
            var gm = config.GetFixtureIndexFromLabel(label);
            int gg = gm.Item1;///label / GroupCnt;
            int ggmod = gm.Item2;/// label % GroupCnt;
            Program.logIt($"SendTestLed++ {gg}:{ggmod}");

            int[] ledindexs = config.CurStripIndex(gg).ToArray();///((Object[])config["stripindexs"]).Cast<int>().ToArray();

            Object[] colors = (Object[])inpt["colors"];
            if (colors.Length == 0)
            {
                return bret;
            }

            if (gg < MAX_Groupt)
            {
                string ss = "";
                var clr = colors[0];
                int[] rgb = ((Object[])clr).Cast<int>().ToArray();
                ss += $"{rgb[0]},{rgb[1]},{rgb[2]},";

                bret = serials[gg].SendData($"T{ledindexs[ggmod]},{ggmod},{ss}\r");
                
            }

            return bret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inpt"></param>
        /// <returns></returns>
        bool SendStatusLED(Dictionary<String, Object> inpt)
        {
            bool bret = false;
            Object[] labelcolors = (Object[])inpt["labels"];
            Dictionary<int, RGB> labelLed = new Dictionary<int, RGB>();
            foreach(var obj in labelcolors)
            {
                Dictionary<String, Object> item = (Dictionary<String, Object>)obj;
                int ll = Convert.ToInt32(item["label"]) - 1;

                int[] rgba = ((Object[])item["color"]).Cast<int>().ToArray();
                RGB rgb;
                rgb.r = (byte)rgba[0];
                rgb.g = (byte)rgba[1];
                rgb.b = (byte)rgba[2];

                labelLed[ll] = rgb;

                status_leds[ll] = rgb; 
            }

            for(int i = 0; i < config.Count; /*MAX_Groupt;*/ i++)
            {
                bool bFind = false;
                int n = config.GetLessCurlabels(i);
                for (int j= 0; /*i*GroupCnt;*/ j < config.CurLabels(i).Count; /*(i+1)*GroupCnt;*/ j++)
                {
                    if (labelLed.ContainsKey(n+j))
                    {
                        bFind = true;
                        labelLed.Remove(n+j);
                       
                    }
                }
                if (bFind)
                {
                    string ss = "";
                    n = config.GetLessCurlabels(i);
                    for (int j = 0; /*i*GroupCnt;*/ j < config.CurLabels(i).Count; /*(i+1)*GroupCnt;*/ j++)
                    {
                        ss += status_leds[n+j].ToString();
                    }
                    bret = serials[i].SendData($"B0,{config.CurLabels(i).Count},{ss}\r");
                }
            }
            return bret;
        }

        bool SendStrip(Dictionary<String, Object> inpt)
        {
            bool bret = false;
            int label = Convert.ToInt32(inpt["label"]) -1;
            var gm = config.GetFixtureIndexFromLabel(label);
            int gg = gm.Item1;///label / GroupCnt;
            int ggmod = gm.Item2;/// label % GroupCnt;
            Program.logIt($"SendStrip++ {gg}:{ggmod}");

            int[] ledindexs = config.CurStripIndex(gg).ToArray(); ///((Object[])config["stripindexs"]).Cast<int>().ToArray();

            Object[] colors = (Object[])inpt["colors"];
            if (gg < MAX_Groupt)
            {
                //if(oldStripSelect != gg)  //why A0,0, Protocal no support.
                //{
                //    serials[oldStripSelect].SendData($"A0,0\r");
                //    oldStripSelect = gg;
                //}
                //for (int i = 0; i < gg; i++)
                //{
                //    //serials[i].SendData($"A0,0\r");
                //}
                string ss = "";
                foreach(var clr  in colors)
                {
                    int[] rgb = ((Object[])clr).Cast<int>().ToArray();
                    ss += $"{rgb[0]},{rgb[1]},{rgb[2]},";
                }
                bret = serials[gg].SendData($"A{ledindexs[ggmod]},{colors.Length},{ss}\r");
                //for (int i = gg+1; i < MAX_Groupt; i++)
                //{
                //   //serials[i].SendData($"A0,0\r");
                //}
            }
            return bret;
        }

        public void Uninit()
        {
            Program.logIt("Uninit ++");
            Cleanup();
            mStopEvent.Set();
            lock (serials)
            {
                foreach (var ss in serials)
                {
                    ss.ExitThread();
                }
            }
        }
    }
}

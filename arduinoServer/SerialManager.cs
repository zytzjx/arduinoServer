using arduinoServer.Properties;
using System;
using System.Collections.Generic;
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
        private Dictionary<String, Object> config = new Dictionary<string, object>();
        private EventWaitHandle mStopEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
        private int GroupCnt = 0;
        private int MAX_Groupt = 0;
        RGB[] status_leds;

        private Dictionary<int, TcpClient> callbacklist = new Dictionary<int, TcpClient>();

        private int oldStripSelect = 0;
        public List<string> GetColorSensorPorts()
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

        public void SendDatatoCallBack(String message)
        {
            if (!String.IsNullOrEmpty(message))
            {
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


        public void Init()
        {
            String s = System.IO.File.ReadAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Serialconfig.json"));
            var serializer = new JavaScriptSerializer();
            config = (Dictionary<String, Object>)serializer.DeserializeObject(s);

            var seriportLabels = new JavaScriptSerializer();
            var portlabels = (Dictionary<String, Object>)serializer.DeserializeObject(Settings.Default.PORTLABELS);

            var seri = new JavaScriptSerializer();
            var StripIndex = (Dictionary<String, Object>)serializer.DeserializeObject(Settings.Default.STRIPINDEX);

            config["portlabel"] = portlabels["portlabel"];
            config["stripindexs"] = StripIndex["stripindexs"];

            //GroupCnt = (((Object[])config["portlabel"]).ToArray(typeof(int))).Length;
            GroupCnt = ((Object[])config["portlabel"]).Cast<int>().ToArray().Length;

            List<string> sComs = GetColorSensorPorts();
            Program.logIt($"serial coms count {sComs.Count}");
            if (config.ContainsKey("serialports"))
            {
                if (((Object[])config["serialports"]).Length > 1)
                {
                    sComs = ((Object[])config["serialports"]).Cast<String>().ToList();
                }
            }
            

            //Object[] sComArray = sComs.ToArray();//{ "COM3" };
           

            //Regex r = new Regex(@"^USB-SERIAL CH340 \(([COM\d]+)\)$|^([COM\d]+)$");
            int index = 0;
            for (int i = 0; i< sComs.Count; i++){
                String sComName = sComs[i];
                //Match m = r.Match(sComs[i]);
                //if (m.Success)
                {
                    SerialMonitor sertmp = new SerialMonitor();
                    sertmp.Index = index++;
                    //String sComName = m.Groups[1].Value.ToString();
                    //if (String.IsNullOrEmpty(sComName))
                    //{
                    //    sComName = m.Groups[0].Value.ToString();
                    //}
                    Program.logIt($"{sComName} opening");
                    int iretry = 5;
                    while (!sertmp.Open(sComName))
                    {
                        Thread.Sleep(1000);
                        if (iretry-- < 0) break;
                    }
                    serials.Add(sertmp);
                    waitHandles.Add(sertmp.mDataEvent);
                }
            }

            MAX_Groupt = serials.Count();

            status_leds = new RGB[GroupCnt * MAX_Groupt];
            for(int i = 0; i < GroupCnt * MAX_Groupt; i++)
            {
                status_leds[i] = new RGB(0, 0, 0);
            }

            for(int i = 1; i <= arduinoServer.Properties.Settings.Default.MAX_SUPPORT_LABEL; i++)
            {
                buttonstatus[i] = false;
            }

            Thread thread1 = new Thread(MonitorThread);
            thread1.Start();

            EZUSB ezUSB = new EZUSB();

            ezUSB.AddUSBEventWatcher(USBEventHandler, USBEventHandler, new TimeSpan(0, 0, 1));

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
               
            }
        }

        private void AddStatus(SerialMonitor sertmp)
        {
            Dictionary<int, bool> bstatus = new Dictionary<int, bool>();
            sertmp.CopyKeys(bstatus);
            int[] plabels= ((Object[])config["portlabel"]).Cast<int>().ToArray();
            int nLabelStart = sertmp.Index * plabels.Length;
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

        public bool Cleanup()
        {
            bool bret = true;
            foreach(var ser in serials)
            {
                if (!ser.SendData("C\r"))
                {
                    bret = false;
                }

            }
            return bret;
        }

        public Dictionary<int, bool> GetKey(int id)
        {
            Dictionary<int, bool> ret = new Dictionary<int, bool>();
            foreach (var ser in serials)
            {
                if (ser.Index * GroupCnt <= id && id < (ser.Index + 1))
                {
                    Dictionary<int, bool> temp = new Dictionary<int, bool>();
                    ser.CopyLabelKeys(ret, ((Object[])config["portlabel"]).Cast<int>().ToArray());
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
                ser.CopyLabelKeys(temp, ((Object[])config["portlabel"]).Cast<int>().ToArray());
                foreach(var kk in temp)
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
            else
            {
                bret = SendStrip(sendLed);
            }

            Dictionary<string, bool> sss = new Dictionary<string, bool>();
            sss["result"] = bret;

            return sss;
        }

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

            for(int i = 0; i < MAX_Groupt; i++)
            {
                bool bFind = false;
                for(int j=i*GroupCnt;j<(i+1)*GroupCnt; j++)
                {
                    if (labelLed.ContainsKey(j))
                    {
                        bFind = true;
                        labelLed.Remove(j);
                       
                    }
                }
                if (bFind)
                {
                    string ss = "";
                    for (int j = i * GroupCnt; j < (i + 1) * GroupCnt; j++)
                    {
                        ss += status_leds[j].ToString();
                    }
                    bret = serials[i].SendData($"B0,{GroupCnt},{ss}\r");
                }
            }
            return bret;
        }

        bool SendStrip(Dictionary<String, Object> inpt)
        {
            bool bret = false;
            int label = Convert.ToInt32(inpt["label"]) -1;

            int gg = label / GroupCnt;
            int ggmod = label % GroupCnt;

            int[] ledindexs = ((Object[])config["stripindexs"]).Cast<int>().ToArray();

            Object[] colors = (Object[])inpt["colors"];
            if (gg < MAX_Groupt)
            {
                if(oldStripSelect != gg)
                {
                    serials[oldStripSelect].SendData($"A0,0\r");
                    oldStripSelect = gg;
                }
                for (int i = 0; i < gg; i++)
                {
                    //serials[i].SendData($"A0,0\r");
                }
                string ss = "";
                foreach(var clr  in colors)
                {
                    int[] rgb = ((Object[])clr).Cast<int>().ToArray();
                    ss += $"{rgb[0]},{rgb[1]},{rgb[2]},";
                }
                bret = serials[gg].SendData($"A{ledindexs[ggmod]},{colors.Length},{ss}\r");
                for (int i = gg+1; i < MAX_Groupt; i++)
                {
                   //bret =  serials[i].SendData($"A0,0\r");
                }
            }
            return bret;
        }

        public void Uninit()
        {
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

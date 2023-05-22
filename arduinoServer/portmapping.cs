using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace arduinoServer
{

    class PortMapping
    {
        const string VID340 = "1A86";
        const string PID340 = "7523";
        //Console.WriteLine(GetPnpDeviceIdFromUsbId(vid, pid));
        public List<string> GetPnpDeviceIdFromUsbId(string vid= "1A86", string pid = "7523")
        {
            List<string> items = new List<string>();
            string pnpDeviceId = null;
            var usbSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice");
            foreach (var usb in usbSearcher.Get())
            {
                var antecedent = usb.GetPropertyValue("Antecedent") as string;
                var dependent = usb.GetPropertyValue("Dependent") as string;
                //Console.WriteLine(dependent);
                //Regex.IsMatch(dependent, Regex.Escape($"VID_{vid}"), RegexOptions.IgnoreCase);
                //Regex.IsMatch(dependent, Regex.Escape($"PID_{pid}"), RegexOptions.IgnoreCase);
                if (dependent.Contains($"VID_{vid}") && dependent.Contains($"PID_{pid}"))
                {
                    //\\JEFFREYPC\root\cimv2:Win32_PnPEntity.DeviceID="USB\\VID_1A86&PID_7523\\6&223C545B&0&1"
                    string deviceId = dependent.Substring(dependent.IndexOf("=\"") + 2).TrimEnd('\"');
                    //Console.WriteLine(deviceId);
                    var pnpSearcher = new ManagementObjectSearcher($"SELECT * FROM Win32_PnPEntity WHERE DeviceID='{deviceId}'");
                    foreach (var pnp in pnpSearcher.Get())
                    {
                        pnpDeviceId = pnp.GetPropertyValue("PNPDeviceID") as string;
                        //Console.WriteLine(pnpDeviceId);
                        items.Add(pnpDeviceId);
                        break;
                    }
                    //break;
                }
            }
            return items;
        }

        private String GetComFromInstanceID(string sinstanceid)
        {
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Enum\{sinstanceid}\Device Parameters", false);
            String value = (String)myKey.GetValue("PortName");
            myKey.Close();
            return value;
        }

        private String GetLocationpathsFromInstanceID(string sinstanceid)
        {
            string locs = "";
            IntPtr hDevInfo = windowsAPI.SetupDiCreateDeviceInfoList(IntPtr.Zero, IntPtr.Zero);
            if (hDevInfo != IntPtr.Zero)
            {
                windowsAPI.SP_DEVINFO_DATA devInfoData = new windowsAPI.SP_DEVINFO_DATA();
                devInfoData.cbSize = (UInt32)Marshal.SizeOf(typeof(windowsAPI.SP_DEVINFO_DATA));
                if (windowsAPI.SetupDiOpenDeviceInfo(hDevInfo, sinstanceid, IntPtr.Zero, 0, ref devInfoData))
                {
                    windowsAPI.DEVPROP_TYPE type;
                    uint sz = 4096;
                    byte[] buffer = new byte[sz];
               
                    if (windowsAPI.SetupDiGetDeviceProperty(hDevInfo, ref devInfoData, ref windowsAPI.DEVPKEY_Device_LocationPaths, out type, buffer, sz, ref sz, 0))
                    {
                        locs = UTF8Encoding.Unicode.GetString(buffer).Split(new char[]{ '\0' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    }
                }
                else
                {
                    Program.logIt($"SetupDiOpenDeviceInfo error={windowsAPI.GetLastError()}");
                }

                windowsAPI.SetupDiDestroyDeviceInfoList(hDevInfo);
            }
            
            return locs;
        }
        

        public Dictionary<String, String> GetCh340Serial()
        {
            Dictionary<String, String> lkcom = new Dictionary<string, string>();
            List<string> instances =  GetPnpDeviceIdFromUsbId();
            foreach(var si in instances)
            {
                String ss = GetLocationpathsFromInstanceID(si);
                if (!String.IsNullOrEmpty(ss))
                {
                    string sPort = GetComFromInstanceID(si);
                    lkcom[ss] = sPort;
                }
            }
            return lkcom;
        }
    }
}

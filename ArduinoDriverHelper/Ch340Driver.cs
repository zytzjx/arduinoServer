using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoDriverHelper
{
    class Ch340Driver
    {
        public static string[] runExe(string exeFilename, string param, out int exitCode, System.Collections.Specialized.StringDictionary env = null, int timeout = 180 * 1000)
        {
            List<string> ret = new List<string>();
            exitCode = 1;
            Program.logIt($"[runExe]: ++ exe={exeFilename}, param={param}");
            try
            {
                if (System.IO.File.Exists(exeFilename))
                {
                    System.Threading.AutoResetEvent ev = new System.Threading.AutoResetEvent(false);
                    Process p = new Process();
                    p.StartInfo.FileName = exeFilename;
                    p.StartInfo.Arguments = param;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;
                    if (env != null && env.Count > 0)
                    {
                        foreach (DictionaryEntry de in env)
                        {
                            p.StartInfo.EnvironmentVariables.Add(de.Key as string, de.Value as string);
                        }
                    }
                    p.OutputDataReceived += (obj, args) =>
                    {
                        if (args.Data == null)
                            ev.Set();
                        else
                        {
                            Program.logIt($"[runExe]: {args.Data}");
                            ret.Add(args.Data);
                        }
                    };
                    p.Start();
                    p.BeginOutputReadLine();
                    if (p.WaitForExit(timeout))
                    {
                        ev.WaitOne(timeout);
                        if (!p.HasExited)
                        {
                            exitCode = 1460;
                            p.Kill();
                        }
                        else
                            exitCode = p.ExitCode;
                    }
                    else
                    {
                        if (!p.HasExited)
                        {
                            p.Kill();
                        }
                        exitCode = 1460;
                    }
                }
                else
                {
                    Program.logIt($"[runExe]: {exeFilename} is not exist");
                }
            }
            catch (Exception ex)
            {
                Program.logIt($"[runExe]: {ex.Message}");
                Program.logIt($"[runExe]: {ex.StackTrace}");
            }
            Program.logIt($"[runExe]: -- ret={exitCode}");
            return ret.ToArray();
        }

        static String Key2Key(String skey)
        {
            /*
            Published Name:     oem54.inf
            Original Name:      lgandgps64.inf
            Provider Name:      LG Electronics Inc.
            Class Name:         Ports
            Class GUID:         {4d36e978-e325-11ce-bfc1-08002be10318}
            Driver Version:     11/30/2010 2.2.0.0
            Signer Name:        Microsoft Windows Hardware Compatibility Publisher
            */
            if (skey == "Published Name") return "published";
            else if (skey == "Original Name") return "original";
            else if (skey == "Provider Name") return "provider";
            else if (skey == "Class Name") return "class";
            else if (skey == "Class GUID") return "GUID";
            else if (skey == "Driver Version") return "version";
            else if (skey == "Signer Name") return "signer";
            else return "";
        }
        static List<Dictionary<string, string>> getCH340DriverInfo()
        {
            Program.logIt($"getCH340DriverInfo: ++");
            List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> driver_info = new List<Dictionary<string, string>>();
            string tool = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "pnputil.exe");
            int exit_code;
            string[] lines = runExe(tool, "/enum-drivers /class Ports", out exit_code);
            Dictionary<string, string> dd = new Dictionary<string, string>();
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    if (dd.Count > 0)
                    {
                        driver_info.Add(dd);
                        dd = new Dictionary<string, string>();
                    }
                }
                else
                {
                    int pos = line.IndexOf(':');
                    if (pos > 0)
                    {
                        string key = line.Substring(0, pos);
                        string value = line.Substring(pos + 1);
                        key = Key2Key(key);
                        if (!String.IsNullOrEmpty(key))
                        {
                            dd[key] = value.Trim();
                        }
                    }
                }
            }

            // return all driver provide by Apple
            foreach (Dictionary<string, string> d in driver_info)
            {
                if (d.ContainsKey("provider") && d["provider"].Contains("wch.cn"))
                {
                    ret.Add(d);
                }
            }
            Program.logIt($"Dump Ch340 Driver: (total: {ret.Count()})");
            int idx = 1;
            foreach (Dictionary<string, string> d in ret)
            {
                Program.logIt($"#{idx}:");
                foreach (KeyValuePair<string, string> kvp in d)
                {
                    Program.logIt($"\t{kvp.Key}={kvp.Value}");
                }
            }
            Program.logIt($"getCH340DriverInfo: --");
            return ret;
        }

        public static int prepareCh340Driver(System.Collections.Specialized.StringDictionary args)
        {
            int ret = 0;
            Program.logIt($"prepareCh340Driver: ++");
            var ch340_drivers = getCH340DriverInfo();
            Boolean bFound = false;
            if (ch340_drivers.Count > 0)
            {
                foreach (Dictionary<string, string> d in ch340_drivers)
                {
                    if (d.ContainsKey("provider") && d["provider"] == "wch.cn")
                    {
                        if (d.ContainsKey("version") && d["version"].Contains("3.5.2019"))
                        {
                            bFound = true;
                            continue;
                        }
                        else
                        {
                            if (d.ContainsKey("published"))
                            {
                                string tool = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.System), "pnputil.exe");
                                int exit_code;
                                string[] lines = runExe(tool, $"/delete-driver {d["published"]} /uninstall /force", out exit_code);
                                if (exit_code == 3010)
                                    ret = 3;
                            }
                        }
                    }
                }
            }
           
            if (!bFound)
            {
                //Install Driver //pnputil /add-driver C:\WCH.CN\CH341SER\CH341SER.INF /install
                string tool = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.System), "pnputil.exe");
                int exit_code;
                string[] lines = runExe(tool, $"/add-driver {Environment.ExpandEnvironmentVariables("%APSTHOME%DriverUpdate\\CH341SER\\CH341SER.inf")} /install", out exit_code);
                ret = exit_code;
            }

            Program.logIt($"prepareCh340Driver: -- ret = {ret}");
            return ret;
        }

    }
}

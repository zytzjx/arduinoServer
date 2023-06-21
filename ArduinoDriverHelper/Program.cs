using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ArduinoDriverHelper
{
    class Program
    {
        public static void logIt(String msg)
        {
            System.Diagnostics.Trace.WriteLine($"[ArduinoDriverHelper]: {msg}");
        }

        public static bool IsAdministrator()
        {
            bool bRet = false;
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch { }
            return bRet;
        }

        static int restartByRDU()
        {
            int ret = 1;
            string fdu = System.IO.Path.Combine(System.Environment.ExpandEnvironmentVariables("%apsthome%"), "fdu.exe");
            string fdu_tmp = System.IO.Path.GetTempFileName();
            if (System.IO.File.Exists(fdu))
            {
                string exe = System.Environment.GetCommandLineArgs()[0];
                int pos = System.Environment.CommandLine.IndexOf(' ', exe.Length);
                string param = System.Environment.CommandLine.Substring(pos + 1);
                XmlTextWriter textWriter = new XmlTextWriter(fdu_tmp, null);
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("runexe");
                textWriter.WriteStartElement("exepath");
                textWriter.WriteString(exe);
                textWriter.WriteEndElement();
                textWriter.WriteStartElement("parameter");
                textWriter.WriteString(param);
                textWriter.WriteEndElement();
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
                textWriter.Close();
                logIt($"Launch FDU: {System.IO.File.ReadAllText(fdu_tmp)}");
                // run
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = fdu;
                p.StartInfo.Arguments = fdu_tmp;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                p.WaitForExit();
                ret = p.ExitCode;
                // detele tmp file
                try { System.IO.File.Delete(fdu_tmp); }
                catch (Exception) { }
            }
            else
            {
                logIt($"{fdu} doesn't exist.");
            }
            return ret;
        }

        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            System.Configuration.Install.InstallContext _args = new System.Configuration.Install.InstallContext(null, args);
            if (_args.IsParameterTrue("debug"))
            {
                System.Console.WriteLine("Wait for debugger, press any key to continue...");
                System.Console.ReadKey();
            }

            logIt(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileVersionInfo.ToString());
            // dump args
            logIt(string.Format("called by arg: ({0})", args.Length));
            foreach (string s in args)
                logIt(s);
            if (!IsAdministrator())
            {
                return restartByRDU();
            }
            return Ch340Driver.prepareCh340Driver(_args.Parameters);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception e1 = (Exception)e.ExceptionObject;
            logIt("UnhandledException caught : " + e1.Message);
            logIt($"UnhandledException Runtime terminating: {e.IsTerminating}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace arduinoServer
{
    class Program
    {
        static public void logIt(string s)
        {
            System.Diagnostics.Trace.WriteLine($"[arduinoServer]:{s}");
        }
        const string androidServer_Event_Name = "ARDUINOSERVER_04122020";

        public static SerialManager SerialManager = new SerialManager();

        static void DoNetshStuff()
        {
            if (Properties.Settings.Default.updateurlacl) return;
            // get full path to netsh.exe command
            var netsh = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "netsh.exe");

            // prepare to launch netsh.exe process
            var startInfo = new ProcessStartInfo(netsh);
            startInfo.Arguments = " http add urlacl url=\"http://+:3420/\" user=\"Everyone\"";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;


            try
            {
                var process = Process.Start(startInfo);
                string stdout = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Boolean bSucc = false;
                if (!String.IsNullOrEmpty(stdout) && stdout.IndexOf("that file already exists") > 0)
                {
                    bSucc = true;
                }
                if (process.ExitCode == 0 || bSucc)
                {
                    Properties.Settings.Default.updateurlacl = true;
                    Properties.Settings.Default.Save();
                }
                
            }
            catch (FileNotFoundException)
            {
                // netsh.exe was missing?
            }
            catch (Win32Exception)
            {
                // user may have aborted the action, or doesn't have access
            }
        }

        static void ZipFiletoFile()
        {
            String sourceFilePath = Environment.ExpandEnvironmentVariables(@"%APSTHOME%arduinoServer.log");
            String destinationFilePath = Environment.ExpandEnvironmentVariables($@"%APSTHOME%logs\backups\arduinoServer_{DateTime.Now.ToString("yyyyMMddThhmmss")}.zip");
            if (!File.Exists(sourceFilePath)) return;
            try
            {
                using (FileStream sourceFileStream = File.OpenRead(sourceFilePath))
                {
                    using (FileStream destinationFileStream = File.Create(destinationFilePath))
                    {
                        using (ZipArchive archive = new ZipArchive(destinationFileStream, ZipArchiveMode.Create))
                        {
                            string entryName = Path.GetFileName(sourceFilePath);
                            ZipArchiveEntry entry = archive.CreateEntry(entryName);

                            using (Stream entryStream = entry.Open())
                            {
                                sourceFileStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
            }
            catch { }
            try
            {
                File.Delete(sourceFilePath);
            }
            catch
            {

            }
        }

        static void prepareArduinoDriver()
        {
            logIt("prepareArduinoDriver ++");
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ArduinoDriverHelper"),
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };
            proc.Start();
            proc.WaitForExit();
            logIt("prepareArduinoDriver --");
        }

        [MTAThread]
        static void Main(string[] args)
        {
            ZipFiletoFile();
            Trace.Listeners.Add(new TextWriterTraceListener(Environment.ExpandEnvironmentVariables(@"%APSTHOME%logs\arduinoServer.log"), "myListener"));
            Trace.AutoFlush = true;
            //Config config = new Config();
            //config.LoadConfigFile(@"E:\Works\Arduino\arduinoServer\arduinoServer\Serialconfig.json");
            //logIt($"{config.LabelCount}");
            //logIt($"{String.Join(":", config.GetComList())}");
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

            System.Threading.EventWaitHandle e = null;
            if (_args.IsParameterTrue("start-service"))
            {
                // start service
                try
                {
                    e = System.Threading.EventWaitHandle.OpenExisting(androidServer_Event_Name);
                    e.Close();
                    logIt("Instance already started.");
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    DoNetshStuff();
                    prepareArduinoDriver();

                    e = new EventWaitHandle(false, EventResetMode.ManualReset, androidServer_Event_Name);
                    //argMap.Add("quitEvent", e);
                    SerialManager.Init();
                    start(args, e);
                    e.Close();
                    //SerialManager clean data
                    Trace.Flush();
                    Trace.Close();
                    ZipFiletoFile();
                }
                catch (Exception) { }
            }
            else if (_args.IsParameterTrue("kill-service"))
            {
                // stop service
                try
                {
                    e = System.Threading.EventWaitHandle.OpenExisting(androidServer_Event_Name);
                    if (e != null)
                        e.Set();
                }
                catch (Exception) { }
            }
            else
            {
                System.Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                System.Console.WriteLine("-start-service: to start the service");
                System.Console.WriteLine("-kill-service: to stop the service");
            }

        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception e1 = (Exception)e.ExceptionObject;
            logIt("UnhandledException caught : " + e1.Message);
            logIt($"UnhandledException Runtime terminating: {e.IsTerminating}");
        }

        static void start(string[] args, EventWaitHandle eh)
        {
            logIt("start: ++");

            using (WebServiceHost host = new WebServiceHost(typeof(SerialService)))
            {
                if (eh != null)
                {
                    WebHttpBinding binding = new WebHttpBinding();
                    EventWaitHandle _quit = eh;
                    host.AddServiceEndpoint(typeof(ISerialServer),
                        binding,
                        "http://127.0.0.1:3420/");
                    host.Open();
                    Console.WriteLine(@"go to http://localhost:3420/ to test");
                    Console.WriteLine(@"Press any key to terminate...");
                    while (!_quit.WaitOne(1000))
                    {
                        if (System.Console.KeyAvailable)
                            _quit.Set();
                    }
                    host.Close();
                    SerialManager.Uninit();
                }
            }

            logIt("start: --");
        }

    }
}

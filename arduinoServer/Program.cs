using System;
using System.Collections.Generic;
using System.Linq;
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
        static void Main(string[] args)
        {
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
                    e = new EventWaitHandle(false, EventResetMode.ManualReset, androidServer_Event_Name);
                    //argMap.Add("quitEvent", e);
                    SerialManager.Init();
                    start(args, e);
                    e.Close();
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


//          SerialManager serialManager = new SerialManager();
//            serialManager.Init();
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

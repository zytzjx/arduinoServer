using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace arduinoServer
{
    [ServiceContract]
    interface ISerialServer
    {
        [OperationContract]
        [WebGet(UriTemplate = "/getkeys")]
        Stream GetKeys();

        [OperationContract]
        [WebGet(UriTemplate = "/getkey?id={id}")]
        Stream GetKey(int id);

        [OperationContract]
        [WebGet(UriTemplate = "/callback?port={port}")]
        Stream callback(int port);

        [OperationContract]
        [WebGet(UriTemplate = "/rmcallback?port={port}")]
        Stream removecallback(int port);

        [OperationContract]
        [WebInvoke(UriTemplate = "/leds", RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        Stream Leds(Stream s);

        [OperationContract]
        [WebGet(UriTemplate = "/cleanup")]
        Stream cleanup();

        [OperationContract]
        [WebGet(UriTemplate = "/ledsOn")]
        Stream ledsOn();

        [OperationContract]
        [WebGet(UriTemplate = "/count")]
        Stream serialcount();


        [OperationContract]
        [WebGet(UriTemplate = "/version")]
        Stream HWVersion();

        [OperationContract]
        [WebGet(UriTemplate = "/serialstatus")]
        Stream SerialStatus();

        [OperationContract]
        [WebGet(UriTemplate = "/querycallback")]
        Stream QueryRegister();

        [OperationContract]
        [WebGet(UriTemplate = "/updateconfig")]
        Stream UpdateConfig();
    }

    public class SerialService : ISerialServer
    {
        public Stream GetKey(int id)
        {
            Dictionary<int, bool> retdata = Program.SerialManager.GetKey(id);

            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(intobjecttoString(retdata)));

            return ret;
        }

        public Stream QueryRegister()
        {
            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(objectToString(Program.SerialManager.GetCallBackPort())));
            return ret;
        }

        public Stream SerialStatus()
        {
            Dictionary<string, bool> retdata = Program.SerialManager.GetStatus();

            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(objectToString(retdata)));

            return ret;
        }

        public Stream HWVersion()
        {
            Dictionary<int, string> retdata = Program.SerialManager.HWVersion();

            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(intstringmaptoString(retdata)));

            return ret;
        }

        public Stream callback(int port)
        {
            bool b = Program.SerialManager.AddCallBackList(port);
            //string aa = b ? "true" : "false";
            Dictionary<String, bool> aa = new Dictionary<string, bool>();
            aa["result"] = b;
            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(objectToString(aa)));
            return ret;
        }

        public Stream removecallback(int port)
        {
            bool b = Program.SerialManager.RemoveCallBackList(port);
            Dictionary<String, bool> aa = new Dictionary<string, bool>();
            aa["result"] = b;
            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(objectToString(aa)));
            return ret;
        }


        public Stream GetKeys()
        {
            Dictionary<int, bool> retdata = Program.SerialManager.GetKeys();
            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(intobjecttoString(retdata)));
            return ret;
        }

        public Stream Leds(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            string s = reader.ReadToEnd();
            Dictionary<string, bool> retdata = Program.SerialManager.SendData(s);
            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(objectToString(retdata)));
            return ret;
        }
        string intstringmaptoString(object o)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            Dictionary<int, string> bid = (Dictionary<int, string>)o;
            int count = bid.Count-1;
            foreach (var sd in bid)
            {
                sb.Append($"\"{sd.Key}\":\"{sd.Value}\"");
                if (count > 0)
                {
                    sb.Append(",");
                }
                count--;
            }
            sb.Append("}");
            return sb.ToString();
        }
        string intobjecttoString(object o)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            Dictionary<int, bool> bid = (Dictionary<int, bool>)o;
            int count = bid.Count - 1;
            foreach (var sd  in bid)
            {
                sb.Append($"\"{sd.Key}\":{sd.Value.ToString().ToLower()}");
                if (count > 0)
                {
                    sb.Append(",");
                }
                count--;
            }
            sb.Append("}");
            return sb.ToString();
        }

        string objectToString(object o)
        {
            var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            string s = jss.Serialize(o);
            return s;
        }

        public Stream cleanup()
        {
            Boolean b = Program.SerialManager.Cleanup();
            Dictionary<String, bool> aa = new Dictionary<string, bool>();
            aa["result"] = b;
            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(objectToString(aa)));
            return ret;
        }

        public Stream ledsOn()
        {
            Boolean b = Program.SerialManager.ledsOn();
            Dictionary<String, bool> aa = new Dictionary<string, bool>();
            aa["result"] = b;
            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(objectToString(aa)));
            return ret;
        }

        public Stream serialcount()
        {
            string count = Program.SerialManager.SerialCountInfo();
            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes($"{{\"result\":{count}}}"));
            return ret;
        }

        public Stream UpdateConfig()
        {
            Program.SerialManager.config.LoadConfigFile(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Serialconfig.json"));
            return new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes($"{{\"result\":\"OK\"}}"));
        }
    }
}

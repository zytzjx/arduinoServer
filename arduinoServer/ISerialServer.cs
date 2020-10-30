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
    }

    public class SerialService : ISerialServer
    {
        public Stream GetKey(int id)
        {
            Dictionary<int, bool> retdata = Program.SerialManager.GetKey(id);

            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(intobjecttoString(retdata)));

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

        string intobjecttoString(object o)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            Dictionary<int, bool> bid = (Dictionary<int, bool>)o;
            foreach(var sd  in bid)
            {
                sb.AppendLine($"\"{sd.Key}\":{sd.Value}");
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
            Stream ret = new MemoryStream(System.Text.UTF8Encoding.Default.GetBytes($"{{\"result\":{b}}}"));
            return ret;
        }
    }
}

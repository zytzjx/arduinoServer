using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace arduinoServer
{
    /// <summary>
    /// Fixture Config 
    /// now Serialports only include one Serial port
    /// Locationpaths only include one location path
    /// </summary>
    public class FixtureConfig
    {
        public List<int> Portlabels;
        public List<int> Stripindexs;
        public List<String> Serialports; 
        public Dictionary<String, String> Locationpaths;
    }

    class Config
    {
        public Dictionary<String, FixtureConfig> FConfigs = new Dictionary<String, FixtureConfig>();
        public void LoadConfigFile(String sFile)
        {
            if (String.IsNullOrEmpty(sFile)||!File.Exists(sFile))
                sFile = System.IO.File.ReadAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Serialconfig.json"));
            var serializer = new JavaScriptSerializer();
            FConfigs = (Dictionary<String, FixtureConfig>)serializer.DeserializeObject(sFile);
        }

        /// <summary>
        /// Fixture Count
        /// </summary>
        public int Count
        {
            get {  return  FConfigs.Count; }
        }

        /// <summary>
        /// Labels Count
        /// </summary>
        private int _labelCount;
        public int LabelCount
        {
            get
            {
                if (_labelCount > 0) return _labelCount;
                int lc = 0;
                foreach(var fc in FConfigs)
                {
                    lc += fc.Value.Portlabels.Count;
                }
                _labelCount = lc;
                return _labelCount;
            }
        }
        /// <summary>
        /// Current Fixture start label
        /// </summary>
        /// <param name="index">fixture index</param>
        /// <returns>current Fixture start label</returns>
        public int GetLessCurlabels(int index)
        {
            int ncount = 0;
            for(int ii = 0; ii<index; ii++)
            {
                string s = ii.ToString();
                if (FConfigs.ContainsKey(s))
                {
                    ncount += FConfigs[s].Portlabels.Count;
                }
                else
                {
                    Program.logIt($"config file error. lost {ii}");
                }
            }
            return ncount;
        }
        /// <summary>
        /// get fixture index from label
        /// </summary>
        /// <param name="labels">current label, label change to 0 start, not greent 1 start</param>
        /// <returns>Fixture index</returns>
        public Tuple<int, int> GetFixtureIndexFromLabel(int labels)
        {
            int index = 0;
            for(int i = 0; i < Count; i++)
            {
                string s = i.ToString();
                if (FConfigs.ContainsKey(s))
                {
                    if (labels < FConfigs[s].Portlabels.Count)
                    {
                        index = i;
                        break;
                    }
                    labels -= FConfigs[s].Portlabels.Count;
                }
                else
                {
                    Program.logIt($"config file error. lost {i}");
                }
            }
            return new Tuple<int, int>(index, labels);
        }
        /// <summary>
        /// get list Comport
        /// </summary>
        /// <returns></returns>
        public List<String> GetComList()
        {
            List<String> coms = new List<string>();
            for (int i = 0; i < Count; i++)
            {
                string s = i.ToString();
                if (FConfigs.ContainsKey(s))
                {
                    coms.Add(FConfigs[s].Serialports[0]);
                }
                else
                {
                    Program.logIt($"config file error. lost {i}");
                }
            }
            return coms;
        }
    }
}

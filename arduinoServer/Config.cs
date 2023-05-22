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
        public List<int> Portlabel;
        public List<int> Stripindexs;
        public List<String> Serialports; 
        public Dictionary<String, String> Serialindex;
        public void Init()
        {
            Portlabel = new List<int>();
            Stripindexs = new List<int>();
            Serialports = new List<string>();
            Serialindex = new Dictionary<string, string>();
        }
    }


    class Config
    {
        public Dictionary<String, FixtureConfig> FConfigs = new Dictionary<String, FixtureConfig>();
        public void LoadConfigFile(String sFile)
        {
            if (String.IsNullOrEmpty(sFile)||!File.Exists(sFile))
                sFile = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Serialconfig.json");

            if (File.Exists(sFile))
            {
                var sjson = File.ReadAllText(sFile);
                var serializer = new JavaScriptSerializer();
                FConfigs = serializer.Deserialize<Dictionary<String, FixtureConfig>>(sjson);
            }
        }

        public void SaveConfigFile(String sFile)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                string sjson = serializer.Serialize(FConfigs);
                File.WriteAllText(sFile, sjson);
            }
            catch (Exception) { }
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
                    lc += fc.Value.Portlabel.Count;
                }
                _labelCount = lc;
                return _labelCount;
            }
        }
        /// <summary>
        /// get label
        /// </summary>
        /// <param name="index">com index</param>
        /// <returns>labels</returns>
        public List<int> CurLabels(int index)
        {
            string s = index.ToString();
            if (FConfigs.ContainsKey(s))
            {
                return  FConfigs[s].Portlabel;
            }
            else
            {
                Program.logIt($"config file error. lost {index}");
            }
            return new List<int>();
        }
        /// <summary>
        /// get strip index array
        /// </summary>
        /// <param name="index">com index</param>
        /// <returns>strip index</returns>
        public List<int> CurStripIndex(int index)
        {
            string s = index.ToString();
            if (FConfigs.ContainsKey(s))
            {
                return FConfigs[s].Stripindexs;
            }
            else
            {
                Program.logIt($"config file error. lost {index}");
            }
            return new List<int>();
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
                    ncount += FConfigs[s].Portlabel.Count;
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
                    if (labels < FConfigs[s].Portlabel.Count)
                    {
                        index = i;
                        break;
                    }
                    labels -= FConfigs[s].Portlabel.Count;
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

        public bool ListsEquivalentDuplicateElements(List<string> listA, List<string> listB)
        {
            // Check if both lists contain the same elements
            if (listA.SequenceEqual(listB))
            {
                return true;
            }

            // Check if listA contains all elements of listB and vice versa
            return listA.OrderBy(x => x).SequenceEqual(listB.OrderBy(x => x)) && listB.OrderBy(x => x).SequenceEqual(listA.OrderBy(x => x));
        }

        public bool AreListsEquivalent(List<string> listA, List<string> listB)
        {
            if (listA == null && listB == null) return true;
            if (listA == null || listB == null) return false;
            var setA = new HashSet<string>(listA);
            var setB = new HashSet<string>(listB);

            // Check if both sets contain the same elements
            if (setA.SetEquals(setB))
            {
                return true;
            }

            // Check if setA contains all elements of setB and vice versa
            return false;
        }

        public bool IsMatchConfig(List<string> serials)
        {
            return AreListsEquivalent(GetComList(), serials);
        }

        public List<string> GetUseCommList(List<string> serials)
        {
            var configlist = GetComList();
            if (AreListsEquivalent(configlist,serials))
            {
                return configlist;
            }

            PortMapping portMapping = new PortMapping();
            var kvmap = portMapping.GetCh340Serial();
            if (kvmap.Count == 0)
            {
                return configlist;
            }
            return GetComListConn(kvmap);
        }

        public List<string> GetComListConn(Dictionary<String, string> kv)
        {
            //key is location path
            List<String> coms = new List<string>();
            for (int i = 0; i < Count; i++)
            {
                string s = i.ToString();
                if (FConfigs.ContainsKey(s))
                {
                    try
                    {
                        if (kv.ContainsKey(FConfigs[s].Serialindex["0"]))
                        {
                            coms.Add(kv[FConfigs[s].Serialindex["0"]]);
                        }
                        else
                        {
                            coms.Add(FConfigs[s].Serialports[0]);
                        }
                    }
                    catch
                    {
                        Program.logIt("config file failed or kv is null");
                    }
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

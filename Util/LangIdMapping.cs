using System;
using System.Collections.Generic;
using System.Xml;
using PullFinanceData.Constants;

namespace PullFinanceData.Util
{
    /// <summary>
    ///     Language Id, Map Mapping
    /// </summary>
    public class LangIdMapping
    {
        private static readonly object m_instanceLock = new object();
        private static LangIdMapping m_instance;
        private readonly Dictionary<string, string> s_IdMap;
        private readonly Dictionary<string, Lang> s_LangMap = new Dictionary<string, Lang>();

        private LangIdMapping(string mappingfile)
        {
            s_IdMap = LoadMap(mappingfile);
        }

        public static LangIdMapping Instance
        {
            get
            {
                lock (m_instanceLock)
                {
                    if (m_instance == null)
                        m_instance = new LangIdMapping(AWDEnvironment.ApplicationPath + ResourceConst.s_LangMapFile);
                }

                return m_instance;
            }
        }


        public ICollection<string> LangIds => s_IdMap.Values;

        private Dictionary<string, string> LoadMap(string mappingfile)
        {
            var hash = new Dictionary<string, string>();
            try
            {
                var xmldoc = XmlUtil.LoadXmlFile(AWDEnvironment.s_DefaultEnviroment, mappingfile);
                foreach (XmlElement elem in xmldoc.SelectNodes("//Language"))
                {
                    var s = elem.GetAttribute("S");
                    var id = elem.GetAttribute("Id");
                    var code = elem.GetAttribute("code");
                    var datetype = elem.GetAttribute("datetype");
                    var regionId = elem.GetAttribute("regionid");
                    var name = elem.Value;
                    hash[s] = id;
                    s_LangMap[s] = new Lang { Name = name, Code = code, Datetype = datetype, RegionId = regionId };
                }
            }
            catch (Exception ex)
            {
                ExceptionUtil.DefaultHandleException(ex);
            }

            return hash;
        }

        internal string GetLangId(string s)
        {
            if (!DataTypeUtil.IsNull(s) && s_IdMap.ContainsKey(s))
                return s_IdMap[s];
            return null;
        }

        private Lang GetLang(string id)
        {
            if (id == null)
                id = "";
            if (s_LangMap.ContainsKey(id))
                return s_LangMap[id];
            return null;
        }

        public string GetLangName(string id)
        {
            var obj = GetLang(id);
            return obj == null ? null : obj.Name;
        }

        public string GetLangCode(string id)
        {
            if (!string.IsNullOrEmpty(id) && id.IndexOf("-") > 0)
                return id;
            var obj = GetLang(id);
            return obj == null ? null : obj.Code;
        }

        public string GetLangDateType(string id)
        {
            var obj = GetLang(id);
            return obj == null ? null : obj.Datetype;
        }

        public string GetRegionId(string id)
        {
            var obj = GetLang(id);
            return obj == null ? null : obj.RegionId;
        }

        public string GetRegionIdByLangeId(string langeId)
        {
            foreach (var key in s_IdMap.Keys)
                if (langeId == s_IdMap[key])
                    return GetRegionId(key);
            return string.Empty;
        }

        #region Nested type: Lang

        internal class Lang
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public string Datetype { get; set; }
            public string RegionId { get; set; }
        }

        #endregion
    }
}
using PullFinanceData.Util;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace PullFinanceData.DataObj
{
    public class AppConfiguration
    {
        private static AppConfiguration s_instance;
        private static readonly object s_lock = new object();

        private Dictionary<string, string> m_settings;

        private static string ConfigFileLocation
        {
            get { return ConfigurationManager.AppSettings["AppSettingsConfigFile"] ?? @"C:\AWDServerConfig\AppSettings.config"; }
        }

        private AppConfiguration()
        {
            m_settings = new Dictionary<string, string>();

            var xmlDoc = XmlUtil.LoadXmlFile(ConfigFileLocation);
            var nodeList = xmlDoc.SelectNodes("//configuration/mode");
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    var mode = node.Attributes["id"].InnerText;

                    var itemList = node.SelectNodes("add");
                    if (itemList != null)
                    {
                        foreach (XmlNode item in itemList)
                        {
                            m_settings.Add(mode.ToString() + "_" + item.Attributes["key"].InnerText, item.Attributes["value"].InnerText);
                        }
                    }
                }
            }
        }

        public static AppConfiguration Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        s_instance = s_instance ?? new AppConfiguration();
                    }
                }

                return s_instance;
            }
        }

        public string GetValue(ProductMode mode, string key)
        {
            key = mode.ToString() + "_" + key;
            if (m_settings.ContainsKey(key))
            {
                return m_settings[key];
            }

            return string.Empty;
        }
    }
}
using DataPuller.DataObj;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace PullFinanceData.Util
{
    /// <summary>
    ///     XmlUtil contas Xml helper functions.
    /// </summary>
    public class XmlUtil
    {
        public const string openTagL = "<";
        public const string openTagR = ">";
        public const string closeTagL = "</";
        public const string closeTagR = ">";
        public const string openCloseTag = "/>";

        public static ArrayList GetListAttribute(XmlElement elem, string xpath, string attr)
        {
            var list = new ArrayList();
            var nodes = elem.SelectNodes(xpath);
            foreach (XmlNode node in nodes)
            {
                var val = GetAttributeValue((XmlElement)node, attr);
                if (val != null)
                    list.Add(val);
            }

            return list;
        }

        public static string GetNodeValue(XPathNavigator nav, string path, string defaultValue = null)
        {
            var node = nav == null ? null : nav.SelectSingleNode(path);
            return node == null ? defaultValue : node.Value;
        }

        public static string GetAttributeValue(XmlNode xmlNode, string tagName, string name)
        {
            if (xmlNode == null)
                return null;
            return GetAttributeValue((XmlElement)xmlNode.SelectSingleNode(tagName), name);
        }

        /// <summary>
        ///     Get an attribute of a XmlNode.
        /// </summary>
        /// <param name="node">Node to query.</param>
        /// <param name="name">name of the attribute.</param>
        /// <returns>Attribute value or blank if not found.</returns>
        public static string GetAttributeValue(XmlNode node, string name)
        {
            var attribute = node.Attributes[name];
            return attribute != null ? attribute.Value : string.Empty;
        }

        public static bool doesAttributeExist(XmlNode node, string name)
        {
            return node.Attributes.GetNamedItem(name) != null;
        }

        public static string GetAttributeValue(XmlElement xml, string name)
        {
            if (xml == null)
                return null;
            var attr = xml.Attributes[name];
            if (attr == null) return null;
            return attr.Value;
        }

        public static string GetTextValue(XmlNode xmlnode, string name, string defaultValue = null)
        {
            if (xmlnode == null)
                return null;
            var node = xmlnode.SelectSingleNode(name);
            if (node == null) return defaultValue;
            return node.InnerText;
        }

        public static string[] GetTextValues(XmlNode xmlnode, string name, string defaultValue = null)
        {
            if (xmlnode == null)
                return null;
            var nodes = xmlnode.SelectNodes(name);
            if (nodes == null)
                return null;
            var texts = new string[nodes.Count];
            for (var i = 0; i < nodes.Count; i++) texts[i] = nodes[i].InnerText;
            return texts;
        }

        public static string GetTextValue(XmlNode xmlnode, string name, XmlNamespaceManager xmn)
        {
            if (xmlnode == null)
                return null;
            var node = xmlnode.SelectSingleNode(name, xmn);
            if (node == null) return null;
            return node.InnerText;
        }

        public static string GetOuterXml(XmlElement xml, string name)
        {
            var node = xml.SelectSingleNode(name);
            if (node == null) return null;
            return node.OuterXml;
        }

        public static string GetInnerXml(XmlElement xml, string name)
        {
            var node = xml.SelectSingleNode(name);
            if (node == null) return null;
            return node.InnerXml;
        }

        public static XmlElement AddElement(XmlNode parent, string name)
        {
            XmlElement elem;
            if (parent is XmlDocument)
                elem = ((XmlDocument)parent).CreateElement(name);
            else
                elem = parent.OwnerDocument.CreateElement(name);
            parent.AppendChild(elem);
            return elem;
        }

        public static XmlElement AddElement(XmlNode parent, string name, string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var elem = AddElement(parent, name);
                elem.InnerText = data;
                return elem;
            }

            return null;
        }

        public static XmlElement AddElement(XmlNode parent, string name, bool b)
        {
            var elem = AddElement(parent, name);
            elem.InnerText = DataTypeUtil.Format(b);
            return elem;
        }

        public static XmlElement AddElement(XmlNode parent, string name, Guid guid)
        {
            var elem = AddElement(parent, name);
            elem.InnerText = guid.ToString();
            return elem;
        }

        public static XmlElement AddElement(XmlNode parent, string name, int i)
        {
            var elem = AddElement(parent, name);
            if (!DataTypeUtil.IsNull(i))
                elem.InnerText = i.ToString();
            return elem;
        }

        public static XmlElement AddElement(XmlNode parent, string name, long l)
        {
            var elem = AddElement(parent, name);
            if (!DataTypeUtil.IsNull(l))
                elem.InnerText = l.ToString();
            return elem;
        }

        public static XmlElement AddElement(XmlNode parent, string name, DateTime dt)
        {
            var elem = AddElement(parent, name);
            if (!DataTypeUtil.IsNull(dt))
                elem.InnerText = DataTypeUtil.Format(dt);
            return elem;
        }

        public static XmlElement GetOrAddElement(XmlNode parent, string name)
        {
            return parent.SelectSingleNode(name) as XmlElement ?? AddElement(parent, name);
        }

        public static XmlElement SetElementValue(XmlNode parent, string name, string data)
        {
            return SetElementValue(parent, name, data, false);
        }

        public static XmlElement SetElementValue(XmlNode parent, string name, string data, bool allowNull)
        {
            if (!DataTypeUtil.IsNull(data) || allowNull)
            {
                var elem = (XmlElement)parent.SelectSingleNode(name);
                if (elem == null)
                    return AddElement(parent, name, data);
                elem.InnerText = data;
                return elem;
            }

            return null;
        }

        public static XmlElement AddElementWithNS(XmlNode parent, string name, string nSpace)
        {
            XmlElement elem;
            if (parent is XmlDocument)
                elem = ((XmlDocument)parent).CreateElement(name, nSpace);
            else
                elem = parent.OwnerDocument.CreateElement(name, nSpace);
            parent.AppendChild(elem);
            return elem;
        }

        public static XmlElement AddElementWithNS(XmlNode parent, string name, string nSpace, string data)
        {
            if (data != null)
            {
                var elem = AddElementWithNS(parent, name, nSpace);
                elem.InnerText = data;
                return elem;
            }

            return null;
        }

        public static XmlElement SetTypeAttribute(XmlElement elem, string type)
        {
            if (elem != null) elem.SetAttribute("dt", "urn:schemas-microsoft-com:datatypes", type);
            return elem;
        }

        public static XmlDocument RenameElement(XmlDocument doc, string oldName, string newName)
        {
            var xmlStr = doc.OuterXml;
            xmlStr = xmlStr.Replace("<" + oldName, "<" + newName);
            xmlStr = xmlStr.Replace("</" + oldName + ">", "</" + newName + ">");
            xmlStr = VerifyXml(xmlStr);
            LoadXml(doc, xmlStr);

            return doc;
        }

        public static XmlDocument ReplaceString(XmlDocument doc, string oldStr, string newStr)
        {
            var xmlStr = doc.OuterXml;
            xmlStr = xmlStr.Replace(oldStr, newStr);
            //xmlStr = VerifyXml(xmlStr);
            LoadXml(doc, xmlStr);

            return doc;
        }

        public static void WriteAttribute(XmlWriter writer, string tag, string s)
        {
            if (!DataTypeUtil.IsNull(s))
            {
                //s = EncodeXmlString(s, true, true, true);    // Value of the tag is just literal and should implement escaping to the invalid characters.
                tag = XmlConvert.VerifyName(tag);
                s = VerifyXml(s);
                writer.WriteAttributeString(tag, s);
            }
        }

        public static void WriteAttributeString(XmlWriter writer, string tag, string s)
        {
            if (s != null)
            {
                tag = XmlConvert.VerifyName(tag);
                s = VerifyXml(s);
                writer.WriteAttributeString(tag, s);
            }
        }

        public static void SetAttribute(XmlElement elem, string tag, string s)
        {
            if (!DataTypeUtil.IsNull(s))
                elem.SetAttribute(tag, s);
        }

        public static void UpdateAttribute(XmlElement elem, string tag, string s)
        {
            if (s != null)
                elem.SetAttribute(tag, s);
        }

        public static void WriteAttribute(XmlWriter writer, string tag, bool b)
        {
            if (b)
                writer.WriteAttributeString(tag, DataTypeUtil.Format(b));
        }

        public static void SetAttribute(XmlElement elem, string tag, bool b)
        {
            if (b)
                elem.SetAttribute(tag, DataTypeUtil.Format(b));
        }

        public static void UpdateAttribute(XmlElement elem, string tag, bool b)
        {
            elem.SetAttribute(tag, DataTypeUtil.Format(b));
        }

        public static void WriteAttribute(XmlWriter writer, string tag, int i)
        {
            if (!DataTypeUtil.IsNull(i))
                writer.WriteAttributeString(tag, i.ToString());
        }

        public static void SetAttribute(XmlElement elem, string tag, int i)
        {
            if (!DataTypeUtil.IsNull(i))
                elem.SetAttribute(tag, i.ToString());
        }

        public static void WriteAttribute(XmlWriter writer, string tag, long l)
        {
            if (!DataTypeUtil.IsNull(l))
                writer.WriteAttributeString(tag, l.ToString());
        }

        public static void SetAttribute(XmlElement elem, string tag, long l)
        {
            if (!DataTypeUtil.IsNull(l))
                elem.SetAttribute(tag, l.ToString());
        }

        public static void WriteAttribute(XmlWriter writer, string tag, double d)
        {
            if (!DataTypeUtil.IsNull(d))
                writer.WriteAttributeString(tag, DataTypeUtil.Format(d));
        }

        public static void SetAttribute(XmlElement elem, string tag, double d)
        {
            if (!DataTypeUtil.IsNull(d))
                elem.SetAttribute(tag, DataTypeUtil.Format(d));
        }

        public static void WriteAttribute(XmlWriter writer, string tag, DateTime dt)
        {
            if (!DataTypeUtil.IsNull(dt))
                writer.WriteAttributeString(tag, DataTypeUtil.Format(dt));
        }

        public static void SetAttribute(XmlElement elem, string tag, DateTime dt)
        {
            if (!DataTypeUtil.IsNull(dt))
                elem.SetAttribute(tag, DataTypeUtil.Format(dt));
        }

        public static void WriteAttribute(XmlWriter writer, string tag, Guid guid)
        {
            if (!DataTypeUtil.IsNull(guid))
                writer.WriteAttributeString(tag, guid.ToString());
        }

        public static void SetAttribute(XmlElement elem, string tag, Guid guid)
        {
            if (!DataTypeUtil.IsNull(guid))
                elem.SetAttribute(tag, guid.ToString());
        }

        public static void WriteRowId(XmlWriter writer, string rowid)
        {
            WriteAttribute(writer, "rowid", rowid);
        }

        public static void AddRowId(XmlElement elem, string rowid)
        {
            elem.SetAttribute("rowid", rowid);
        }

        public static void WriteElement(XmlWriter writer, string tag, DateTime dt)
        {
            tag = XmlConvert.VerifyName(tag);
            if (!DataTypeUtil.IsNull(dt))
                writer.WriteElementString(tag, DataTypeUtil.Format(dt));
        }

        public static void WriteElement(XmlWriter writer, string tag, int i)
        {
            tag = XmlConvert.VerifyName(tag);
            if (!DataTypeUtil.IsNull(i))
                writer.WriteElementString(tag, DataTypeUtil.Format(i));
        }

        public static void WriteElement(XmlWriter writer, string tag, double d)
        {
            tag = XmlConvert.VerifyName(tag);
            if (!DataTypeUtil.IsNull(d))
                writer.WriteElementString(tag, DataTypeUtil.Format(d));
        }

        public static void WriteElement(XmlWriter writer, string tag, string s)
        {
            s = EncodeXmlString(s, true, false, false);
            WriteInnerXml(writer, tag, s);
        }

        public static void WriteInnerXml(XmlWriter writer, string tag, string xml)
        {
            tag = XmlConvert.VerifyName(tag);
            if (!DataTypeUtil.IsNull(xml))
            {
                //writer.WriteElementString(tag, s);
                xml = string.Format("<{0}>{1}</{0}>", tag, xml);
                xml = VerifyXml(xml);
                writer.WriteRaw(xml);
            }
        }

        public static void WriteElement(XmlWriter writer, string tag, Guid g)
        {
            tag = XmlConvert.VerifyName(tag);
            if (!DataTypeUtil.IsNull(g))
                writer.WriteElementString(tag, g.ToString());
        }

        public static void WriteElement(XmlWriter writer, string tag, bool b)
        {
            tag = XmlConvert.VerifyName(tag);
            writer.WriteElementString(tag, DataTypeUtil.Format(b));
        }

        public static string EncodeString(string strBefore)
        {
            if (string.IsNullOrEmpty(strBefore))
                return strBefore;

            var strAfter = strBefore.Replace("&", "&amp;");
            strAfter = strAfter.Replace("'", "&apos;");
            strAfter = strAfter.Replace("<", "&lt;");
            strAfter = strAfter.Replace(">", "&gt;");
            return strAfter;
        }

        public static string EncodeStringV2(string strBefore)
        {
            return EncodeString(strBefore)?.Replace("\"", "&quot;");
        }

        public static string DecodeString(string strBefore)
        {
            if (string.IsNullOrWhiteSpace(strBefore))
                return strBefore;

            var strAfter = strBefore.Replace("&amp;", "&");
            strAfter = strAfter.Replace("&apos;", "'");
            strAfter = strAfter.Replace("&lt;", "<");
            strAfter = strAfter.Replace("&gt;", ">");
            return strAfter;
        }

        public static string DecodeStringV2(string strBefore)
        {
            return DecodeString(strBefore)?.Replace("&quot;", "\"");
        }

        public static string EncodeXmlString(string strBefore)
        {
            return EncodeXmlString(strBefore, false, false, false);
        }

        public static string EncodeXmlString(string strBefore, bool bLtGt)
        {
            return EncodeXmlString(strBefore, bLtGt, false, false);
        }

        public static string EncodeXmlString(string strBefore, bool bLtGt, bool bQuote, bool bApos)
        {
            if (strBefore == null)
                return null;
            var strAfter = string.Copy(strBefore.Replace("&amp;", "&"));
            if (bApos)
                strAfter = string.Copy(strAfter.Replace("&apos;", "'"));
            if (bQuote)
                strAfter = string.Copy(strAfter.Replace("&quot;", "\""));
            if (bLtGt)
            {
                strAfter = string.Copy(strAfter.Replace("&lt;", "<"));
                strAfter = string.Copy(strAfter.Replace("&gt;", ">"));
            }

            //encode
            strAfter = string.Copy(strAfter.Replace("&", "&amp;"));
            if (bApos)
                strAfter = string.Copy(strAfter.Replace("'", "&apos;"));
            if (bQuote)
                strAfter = string.Copy(strAfter.Replace("\"", "&quot;"));
            if (bLtGt)
            {
                strAfter = string.Copy(strAfter.Replace("<", "&lt;"));
                strAfter = string.Copy(strAfter.Replace(">", "&gt;"));
            }

            return strAfter;
        }

        public static XmlReaderSettings GetSetting(int maxCharacters = 0, bool ignoreComments = false)
        {
            var settings = new XmlReaderSettings();
            settings.IgnoreComments = ignoreComments;
            settings.DtdProcessing = DtdProcessing.Prohibit;
            settings.MaxCharactersFromEntities = maxCharacters;
            settings.XmlResolver = null;
            return settings;
        }

        public static XmlDocument LoadXml(byte[] bArr)
        {
            if (bArr == null || bArr.Length == 0)
                return null;
            var dom = new XmlDocument();
            using (var ms = new MemoryStream(bArr))
            {
                using (var reader = GetXmlReader(ms))
                {
                    dom.Load(reader);
                }
            }

            return dom;
        }

        public static XmlDocument LoadXml(Stream stream)
        {
            var dom = new XmlDocument();
            using (var xmlReader = GetXmlReader(stream))
            {
                dom.Load(xmlReader);
            }

            return dom;
        }

        public static XmlDocument LoadXml(XmlDocument dom, Stream stream)
        {
            if (dom == null) dom = new XmlDocument();

            using (var xmlReader = GetXmlTextReader(stream))
            {
                dom.Load(xmlReader);
            }

            return dom;
        }

        public static bool TryLoadXml(string xml, out XmlDocument xmlDoc)
        {
            try
            {
                xmlDoc = LoadXml(xml);
                return true;
            }
            catch
            {
                xmlDoc = null;
                return false;
            }
        }

        public static XmlDocument LoadXml(string xml, int maxCharacters = 0)
        {
            var dom = new XmlDocument();
            return LoadXml(dom, xml, maxCharacters);
        }

        public static XmlDocument LoadXml(XmlDocument dom, string xml, int maxCharacters = 0)
        {
            if (DataTypeUtil.IsNull(xml))
                return null;

            if (dom == null) dom = new XmlDocument();

            var sr = new StringReader(xml);
            var settings = GetSetting(maxCharacters);
            using (var xmlReader = XmlReader.Create(sr, settings))
            {
                dom.Load(xmlReader);
            }

            return dom;
        }

        public static string VerifyXml(string xml)
        {
            //if (DataTypeUtil.IsNull(xml))
            //{
            //    return null;
            //}

            //xml = "<WrapRoot>" + xml + "</WrapRoot>";

            //try
            //{
            //    XmlDocument dom = LoadXml(xml);
            //    return dom.DocumentElement.InnerXml;
            //}
            //catch (Exception)
            //{
            //    return null;
            //}
            return XmlConvert.VerifyXmlChars(xml);
        }

        public static XmlNodeList GetNodes(XmlDocument xmlDoc, string path)
        {
            if (xmlDoc == null) return null;

            var rowList = xmlDoc.DocumentElement.SelectNodes(path);
            return rowList;
        }

        public static XmlDocument LoadXmlFile(string path)
        {
            return LoadXmlFile(AWDEnvironment.s_DefaultEnviroment, path);
        }

        public static XmlDocument LoadXmlFile(string path, bool ignoreComments = false)
        {
            return LoadXmlFile(AWDEnvironment.s_DefaultEnviroment, path, ignoreComments);
        }

        /// <summary>
        ///     Load file based xml. It will throw exception if anything wrong.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static XmlDocument LoadXmlFile(AWDEnvironment environment, string path, bool ignoreComments = false)
        {
            var timer = new Timer();
            var dom = new XmlDocument();

            using (var reader = GetXmlReader(path, ignoreComments))
            {
                dom.Load(reader);
            }

            timer.Stop();
            if (environment != null)
            {
                environment.WriteDebugTimer(timer.ElapsedMillSeconds, "Load Xml File " + path);
                if (environment.TrackingLog != null)
                    environment.TrackingLog.AddLog("LoadXmlFile", null, timer.ElapsedMillSeconds, path);
            }

            return dom;
        }

        public static XmlDocument LoadWebXml(string url)
        {
            return LoadWebXml(AWDEnvironment.s_DefaultEnviroment, url);
        }

        /// <summary>
        ///     Load xml from web link. If something wrong, it will catch exception / log it  and return null .
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static XmlDocument LoadWebXml(AWDEnvironment environment, string url)
        {
            return LoadWebXml(environment, url, 0);
        }

        /// <summary>
        ///     Load xml from web link. If something wrong, it will catch exception / log it  and return null .
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="url"></param>
        /// <param name="timeout">The time-out value in milliseconds</param>
        /// <returns></returns>
        public static XmlDocument LoadWebXml(AWDEnvironment environment, string url, int timeout)
        {
            byte[] bArr = HttpUtil.GetResponseData(environment, url, timeout);
            return LoadXml(bArr);
        }

        /// <summary>
        ///     Load xml from web link. If something wrong, it will catch exception / log it  and return null .
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="url"></param>
        /// <param name="timeout">The time-out value in milliseconds</param>
        /// <param name="headers">The header dictionary</param>
        /// <returns></returns>
        public static XmlDocument LoadWebXml(AWDEnvironment environment, string url, int timeout,
            Dictionary<string, string> headers)
        {
            byte[] bArr = HttpUtil.GetResponseDataWithHeader(environment, url, null, headers, "Get", timeout);
            return LoadXml(bArr);
        }

        public static XmlDocument GenerateErrorXml(int errorCode, string msg)
        {
            return "<error code=\"{0}\"><msg>{1}</msg></error>".FormatToXml(errorCode, msg);
        }

        public static string GetXmlSubsetInclude(string xml, string subsetTag)
        {
            var openTag = CreateOpenTag(subsetTag);
            var closeTag = CreateCloseTag(subsetTag);
            var nPos1 = xml.IndexOf(openTag);
            var nPos2 = xml.IndexOf(closeTag);
            if (nPos2 > nPos1 + openTag.Length)
                return xml.Substring(nPos1, nPos2 - nPos1 + closeTag.Length);
            return "<" + subsetTag + "/>";
        }

        public static string GetXmlSubset(string xml, string subsetTag)
        {
            return GetXmlSubsetInclude(xml, subsetTag);
        }

        public static string CreateOpenTag(string tagName)
        {
            return openTagL + tagName + openTagR;
        }

        public static string CreateCloseTag(string tagName)
        {
            return closeTagL + tagName + closeTagR;
        }

        public static string CreateNode(string tagName, string nodeValue)
        {
            if (nodeValue != null)
                return openTagL + tagName + openTagR + nodeValue + closeTagL + tagName + closeTagR;
            return openTagL + tagName + openTagR + "" + closeTagL + tagName + closeTagR;
        }

        public static string GetNodeValue(XmlDocument xmlDoc, string path)
        {
            var node = xmlDoc.SelectSingleNode(path);
            if (node == null)
                return "";
            return node.InnerText;
        }

        public static XmlDocument RemoveNamespace(XmlDocument doc)
        {
            //string pureXml = Regex.Replace(doc.OuterXml, @"(xmlns:?[^=]*=[""][^""]*[""])", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //var newDoc = new XmlDocument();
            //newDoc.LoadXml(pureXml);
            //return newDoc;
            var newdoc = new XmlDocument();

            using (var sr = new StringReader(doc.OuterXml))
            {
                using (var xtr = new XmlTextReader(sr) { Namespaces = false })
                {
                    newdoc.Load(xtr);
                }
            }

            return newdoc;
        }
        //public static bool GetBoolValue(XmlNode node, string name)
        //{
        //    return DataTypeUtil.ObjectBoolValue(GetTextValue(node, name));
        //}

        //public static DateTime GetDateValue(XmlNode node, string name)
        //{
        //    return DataTypeUtil.ObjectDateValue(GetTextValue(node, name));
        //}
        //public static DateTime GetDateValue(XmlNode node, string name, DateTime defaultValue)
        //{
        //    return DataTypeUtil.ObjectDateValue(GetTextValue(node, name), defaultValue);
        //}

        //public static int GetIntValue(XmlNode node, string name)
        //{
        //    return DataTypeUtil.ObjectIntValue(GetTextValue(node, name));
        //}
        //public static int GetIntValue(XmlNode node, string name, int defaultValue)
        //{
        //    return DataTypeUtil.ObjectIntValue(GetTextValue(node, name), defaultValue);
        //}

        //public static double GetDoubleValue(XmlNode node, string name)
        //{
        //    return DataTypeUtil.ObjectDoubleValue(GetTextValue(node, name));
        //}
        //public static double GetDoubleValue(XmlNode node, string name, double defaultValue)
        //{
        //    return DataTypeUtil.ObjectDoubleValue(GetTextValue(node, name), defaultValue);
        //}

        //public static Guid GetGuidValue(XmlNode node, string name)
        //{
        //    return DataTypeUtil.ObjectGuidValue(GetTextValue(node, name));
        //}

        public static void CreateChildElement(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            var elem = parentNode.SelectSingleNode(name) as XmlElement;
            if (DataTypeUtil.ObjectIsNull(elem))
                elem = xmlDoc.CreateElement(name);
            if (elem != null)
            {
                //value = HttpUtility.HtmlEncode(value);
                value = DecodeString(value);
                elem.InnerText = string.IsNullOrEmpty(value) ? string.Empty : value;
            }

            parentNode.AppendChild(elem);
        }

        public static XmlTextReader GetSecurityXmlTextReader(string filePath)
        {
            return new XmlTextReader(filePath)
            {
                DtdProcessing = DtdProcessing.Prohibit,
                WhitespaceHandling = WhitespaceHandling.Significant,
                Normalization = true,
                XmlResolver = null
            };
        }

        public static XmlTextReader GetXmlTextReader(Stream stream, bool normalization = true)
        {
            return new XmlTextReader(stream)
            {
                DtdProcessing = DtdProcessing.Prohibit,
                WhitespaceHandling = WhitespaceHandling.Significant,
                Normalization = normalization,
                XmlResolver = null
            };
        }

        public static XmlTextReader GetXmlTextReader(StringReader strReader)
        {
            return new XmlTextReader(strReader)
            {
                DtdProcessing = DtdProcessing.Prohibit,
                WhitespaceHandling = WhitespaceHandling.Significant,
                Normalization = true,
                XmlResolver = null
            };
        }

        public static XmlTextReader GetXmlTextReader(string xml)
        {
            return new XmlTextReader(xml, XmlNodeType.Element, null)
            {
                DtdProcessing = DtdProcessing.Prohibit,
                WhitespaceHandling = WhitespaceHandling.Significant,
                Normalization = true,
                XmlResolver = null
            };
        }

        public static XmlReader GetXmlReader(string uri, bool ignoreComments = false)
        {
            var settings = GetSetting(0, ignoreComments);
            var reader = XmlReader.Create(uri, settings);
            return reader;
        }

        public static XmlReader GetXmlReader(Stream stream)
        {
            var settings = GetSetting();
            var reader = XmlReader.Create(stream, settings);
            return reader;
        }

        public static XPathDocument LoadXPathDoc(Stream stream)
        {
            var xmlReader = GetXmlReader(stream);
            var dom = new XPathDocument(xmlReader);
            xmlReader.Close();
            return dom;
        }

        public static XPathDocument LoadXPathDoc(string xml)
        {
            return LoadXPathDoc(new StringReader(xml));
        }

        public static XPathDocument LoadXPathDoc(StringReader reader)
        {
            XmlReader xmlReader = GetXmlTextReader(reader);
            var dom = new XPathDocument(xmlReader);
            xmlReader.Close();
            return dom;
        }

        public static XPathDocument LoadXPathFile(string uri)
        {
            XmlReader xmlReader = GetSecurityXmlTextReader(uri);
            var dom = new XPathDocument(xmlReader);
            xmlReader.Close();
            return dom;
        }

        public static string WriteFragment(IEnumerable<BaseSerializableObj> enumerable, string parentTag = null)
        {
            if (enumerable == null) return string.Empty;
            var list = enumerable as BaseSerializableObj[] ?? enumerable.ToArray();
            if (list.Any()) return string.Empty;
            var sb = new StringBuilder();
            var xs = AWDEnvironment.s_DefaultXmlWriterSettings.Clone();
            xs.WriteEndDocumentOnClose = false;
            xs.Indent = false;
            xs.OmitXmlDeclaration = true;
            xs.Encoding = Encoding.UTF8;
            using (var xw = XmlWriter.Create(sb, xs))
            {
                list.WriteTo(xw, parentTag);
            }

            return sb.ToString();
        }

        public static string WriteFragment<T>(IEnumerable<T> list, Action<XmlWriter, T> action = null,
            string parentTag = null)
            where T : BaseSerializableObj
        {
            if (list == null) return string.Empty;
            var sb = new StringBuilder();
            var xs = AWDEnvironment.s_DefaultXmlWriterSettings.Clone();
            xs.WriteEndDocumentOnClose = false;
            xs.Indent = false;
            xs.OmitXmlDeclaration = true;
            xs.Encoding = Encoding.UTF8;
            using (var xw = XmlWriter.Create(sb, xs))
            {
                list.WriteTo(xw, action, parentTag);
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Add an attribute.if exist, update the attribute.  Will remove the attribute if blank or null.
        /// </summary>
        /// <param name="node">XmlNode to update.</param>
        /// <param name="name">Attribute name to add/update.</param>
        /// <param name="attribValue">Attribute value, will remove attribute if null.</param>
        public static void AddAttribute(XmlNode node, string name, string attribValue)
        {
            if (node != null)
            {
                var attribute = node.Attributes[name];
                if (attribute != null) node.Attributes.Remove(attribute);

                if (attribValue != null)
                {
                    attribute = node.OwnerDocument.CreateAttribute(name);
                    node.Attributes.Append(attribute);
                    attribute.Value = attribValue;
                }
            }
        }

        public static Dictionary<string, DateTime> GetSecIdLastTransDateDictFromInputXml(XmlDocument inputXml)
        {
            var secIdLastTransDateDict = new Dictionary<string, DateTime>();
            foreach (XmlElement elem in inputXml.SelectNodes("//r"))
            {
                var transDate = elem.GetAttribute("lasttransdate");
                if (!string.IsNullOrEmpty(transDate))
                {
                    var secId = elem.GetAttribute("i");
                    var index = secId.IndexOf(";");
                    if (index > 0) secIdLastTransDateDict.Add(secId.Substring(0, index), DateTime.Parse(transDate));
                }
            }

            return secIdLastTransDateDict;
        }

        public static string GetXmlLogText(XmlDocument dom, int limitSize)
        {
            if (dom == null)
                return null;

            try
            {
                var outerXml = dom.OuterXml;
                var logText = outerXml.Length > limitSize ? outerXml.Substring(0, limitSize) : outerXml;

                //Remove new line and tab chars
                var pattern = new Regex("[\t\r\n]");
                return pattern.Replace(logText, "");
            }
            catch (Exception ex)
            {
                return "Error to parse xml document:" + ex.Message;
            }
        }
    }
}


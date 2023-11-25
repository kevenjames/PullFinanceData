using System.Xml;
using DataPuller.Util;

namespace DataPuller.DataObj;

/// <summary>
///     BaseSerializableObj contains common functionality for deserialize object from
///     XmlTextReader, IDataReader, xml string and serialize object to XmlWriter and
///     XmlElement.
/// </summary>
[Serializable]
public abstract class BaseSerializableObj
{
    public XmlTextReader DataSource
    {
        set => Deserialize(value);
    }

    /// <summary>
    ///     It can generate an action to handle the class member which itself is a
    ///     BaseSerializableObj object.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected virtual bool StartElement(string tag, XmlTextReader reader)
    {
        return false;
    }

    /// <summary>
    ///     Deserialize object by set value for different tagName.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="objValue"></param>
    /// <returns>whether found the match or not.</returns>
    public virtual bool SetObjectValue(string tag, object objValue)
    {
        return false;
    }

    /// <summary>
    ///     write the object to the writer.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="closetag">close tag or not</param>
    public virtual void WriteTo(XmlWriter writer, bool closetag)
    {
    }

    /// <summary>
    ///     write cell information about this object like
    ///     <c i="OS01W" v="Standard & Poor's 500" />
    ///     <c i="PM00E" v="-0.90" xp="1" />
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="WriteRowInfo">If set as true, then it will also write Row Information</param>
    public virtual void WriteGridCells(XmlWriter writer, bool WriteRowInfo)
    {
    }

    /// <summary>
    ///     writer the object to XmlDocument object.
    /// </summary>
    /// <param name="parentElem"></param>
    public virtual void WriteTo(XmlElement parentElem)
    {
    }


    public void Deserialize(XmlTextReader reader)
    {
        Deserialize(reader, false);
    }

    public virtual void Deserialize(XmlTextReader reader, bool attributeOnly)
    {
        if (reader == null)
            return;
        var startTagName = reader.Name;
        string elemName = null;
        if (reader.IsEmptyElement)
            attributeOnly = true;
        while (true)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    elemName = reader.Name;
                    if (startTagName == elemName)
                    {
                        var attributeCount = reader.AttributeCount;
                        for (var i = 0; i < attributeCount; i++)
                        {
                            reader.MoveToAttribute(i);
                            SetObjectValue(reader.Name, reader.Value);
                        }

                        if (attributeOnly)
                            return;
                    }
                    else
                    {
                        StartElement(elemName, reader);
                    }

                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    SetObjectValue(elemName, reader.Value);
                    break;
                case XmlNodeType.EndElement:
                    if (reader.Name == startTagName)
                        return; // process one object done.
                    break;
            }

            if (!reader.Read())
                return; //should not reach here.
        }
        //should not reach here.
    }

    public void Deserialize(string[] fieldNames, object[] values)
    {
        for (var i = 0; i < fieldNames.Length; i++)
            if (!DataTypeUtil.ObjectIsNull(values[i]))
                SetObjectValue(fieldNames[i], values[i]);
    }

    public void Deserialize(string xml)
    {
        using (var reader = XmlUtil.GetXmlTextReader(xml))
        {
            reader.MoveToContent();
            Deserialize(reader);
        }
    }

    public void WriteTo(XmlWriter writer)
    {
        WriteTo(writer, true);
    }
}
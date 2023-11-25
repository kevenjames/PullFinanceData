using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using System.Xml;
using System;

namespace PullFinanceData.Util
{
    public class StreamUtil
    {
        /// <summary>
        /// Read from stream to fill buffer.
        /// </summary>
        /// <param name="s">stream to read from</param>
        /// <param name="buf">buffer to fill</param>
        /// <param name="off">Offset to start reading</param>
        /// <param name="len">len of data to read</param>
        /// <returns>count of bytes read</returns>
        public static int ReadAll(Stream s, byte[] buf, int off, int len)
        {
            int read = off;
            int count;
            do
            {
                count = s.Read(buf, read, len - read);
                read += count;
            }
            while (count > 0 && read < len);
            return read - off;
        }

        public static byte[] ReadAll(Stream s)
        {
            byte[] res;
            using (var ms = ToMemoryStream(s))
            {
                //ms.Close();
                res = ms.ToArray();
            }
            return res;
        }

        /// <summary>
        /// Read from stream i and write to stream o.
        /// </summary>
        /// <param name="inStream">input stream</param>
        /// <param name="outStream">output stream</param>
        /// <returns>count of byte transferred</returns>
        public static int TransferAll(Stream inStream, Stream outStream)
        {
            int read;
            const int bufferLen = 1024;
            int length = 0;
            var buf = new byte[bufferLen];
            while ((read = inStream.Read(buf, 0, bufferLen)) > 0)
            {
                outStream.Write(buf, 0, read);
                length += read;
            }
            return length;
        }

        public static void WriteMSB(Stream s, uint dw)
        {
            var data = new byte[4];
            data[0] = (byte)dw;
            data[1] = (byte)(dw >> 8);
            data[2] = (byte)(dw >> 16);
            data[3] = (byte)(dw >> 24);
            s.Write(data, 0, 4);
        }

        public static uint ReadMSB(Stream s)
        {
            var data = new byte[4];
            int read = ReadAll(s, data, 0, 4);
            Debug.Assert(4 == read);
            return ReadMSB(data, 0);
        }

        public static uint ReadMSB(byte[] data, int offset)
        {
            return ((data[offset]) | (((uint)data[offset + 1]) << 8) | (((uint)data[offset + 2]) << 16) |
                    (((uint)data[offset + 3]) << 24));
        }

        public static string ReadUTF8(Stream s)
        {
            var stm = new MemoryStream();
            int len = 0;
            int b;
            while ((b = s.ReadByte()) > 0)
            {
                stm.WriteByte((byte)b);
                len++;
            }
            stm.Close();
            return Encoding.UTF8.GetString(stm.GetBuffer(), 0, len);
        }

        public static string ReadUTF8(Stream s, int byteLen)
        {
            var data = new byte[byteLen];
            int read = ReadAll(s, data, 0, byteLen);
            Debug.Assert(read == byteLen);
            return Encoding.UTF8.GetString(data);
        }

        public static string ReadUTF8(byte[] data, int offset)
        {
            //var stm = new MemoryStream();
            //byte b;
            //while((b = data[offset]) != '\0')
            //{
            //    stm.WriteByte(b);
            //    offset++;
            //}
            //stm.Close();
            //return Encoding.UTF8.GetString(stm.GetBuffer());
            if (data == null || data.Length == 0)
            {
                return null;
            }
            return Encoding.UTF8.GetString(data, offset, data.Length - offset);
        }


        public static int GetEstimateGzipSize(int originalSize)
        {
            return 21 + originalSize / 4;
        }

        // The value of this property is the number of minutes intervals 
        // that have elapsed since 12:00 A.M., January 1, 0001.
        public static uint GetIntTimeStamp(DateTime dt)
        {
            return (uint)(dt.Ticks / 10000000 / 60);
        }

        public static XmlDocument LoadXmlDocument(Stream inStream)
        {
            var dom = new XmlDocument();
            try
            {
                dom = XmlUtil.LoadXml(inStream);
            }
            catch (Exception ex)
            {
                ExceptionHandler.RethrowException(ex, null, "Xml Invalid");
            }
            finally
            {
                inStream.Close();
            }
            return dom;
        }

        public static XmlDocument LoadXmlDocument(Stream stream, XmlReaderSettings settings)
        {
            if (stream != null)
            {
                if (settings == null)
                    settings = new XmlReaderSettings
                    {
                        XmlResolver = null,
                        DtdProcessing = DtdProcessing.Prohibit,
                        MaxCharactersFromEntities = 0,
                        MaxCharactersInDocument = 0,
                    };
                using (var reader = XmlReader.Create(stream, settings))
                {
                    var doc = new XmlDocument();
                    doc.Load(reader);
                    return doc;
                }
            }
            return null;
        }

        public static byte[] ReadFile(string fileName)
        {
            return File.ReadAllBytes(fileName);
        }

        public static int TransferStream(Stream inStream, Stream outStream)
        {
            int read;
            const int bufferLen = 1024;
            int length = 0;
            var buf = new byte[bufferLen];
            while ((read = inStream.Read(buf, 0, bufferLen)) > 0)
            {
                outStream.Write(buf, 0, read);
                length += read;
            }
            inStream.Close();
            return length;
        }

        public static MemoryStream ToMemoryStream(Stream inStream)
        {
            var ms = new MemoryStream();
            int read;
            const int bufferLen = 1024;
            int length = 0;
            var buf = new byte[bufferLen];
            while ((read = inStream.Read(buf, 0, bufferLen)) > 0)
            {
                ms.Write(buf, 0, read);
                length += read;
            }
            //inStream.Close();
            ms.Position = 0;
            return ms;
        }

        public static void WriteBinaryData(Stream innerStream, Stream outPutStream)
        {
            using (BinaryReader reader = new BinaryReader(innerStream))
            {
                using (BinaryWriter writer = new BinaryWriter(outPutStream))
                {
                    try
                    {
                        while (true)
                        {
                            byte[] buffer = reader.ReadBytes(1024);
                            if (buffer.Length == 0)
                            {
                                break;
                            }
                            writer.Write(buffer);
                        }
                    }
                    catch (EndOfStreamException)
                    {
                    }

                    writer.Flush();
                }
            }
        }
    }
}
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System;
using ICSharpCode.SharpZipLib.Zip;

namespace PullFinanceData.Util
{
    public enum CompressOption
    {
        None = 0,
        Gzip,
        Zip
    }

    public class ZipUtil
    {
        private const int ZIP_LEAD_BYTES = 0x04034b50;
        private const ushort GZIP_LEAD_BYTES = 0x8b1f;

        public static bool IsZipped(byte[] bArr) => bArr != null && bArr.Length >= 4 && BitConverter.ToInt32(bArr, 0) == ZIP_LEAD_BYTES;
        public static bool IsGzipped(byte[] bArr) => bArr != null && bArr.Length >= 2 && BitConverter.ToUInt16(bArr, 0) == GZIP_LEAD_BYTES;

        public static byte[] DoZip(byte[] bArr)
        {
            return DoZip(bArr, "AWSZip.txt");
        }

        public static byte[] DoZip(byte[] bArr, string fileName)
        {
            if (bArr == null)
                return null;
            byte[] res;
            using (var ms = new MemoryStream(bArr.Length / 6))
            {
                using (var zos = new ZipOutputStream(ms))
                {
                    zos.PutNextEntry(new ZipEntry(fileName));
                    zos.Write(bArr, 0, bArr.Length);
                    zos.Finish();
                }
                res = ms.ToArray();
            }
            return res;
        }

        public static byte[] DoZipStream(Stream stream, string fileName)
        {
            if (stream == null)
                return null;
            byte[] res;
            using (var ms = new MemoryStream())
            {
                using (var zos = new ZipOutputStream(ms))
                {
                    zos.PutNextEntry(new ZipEntry(fileName));
                    TransferAll(stream, zos);
                    zos.Finish();
                }
                res = ms.ToArray();
            }
            return res;
        }

        public static int TransferAll(System.IO.Stream i, System.IO.Stream o)
        {
            byte[] buf = new byte[1024];
            int read = 0;
            int count = 0;
            do
            {
                count = i.Read(buf, 0, buf.Length);
                read += count;
                o.Write(buf, 0, count);
            } while (count > 0);
            return read;
        }

        public static byte[] DoUnZip(byte[] bArr)
        {
            if (bArr == null)
                return null;
            byte[] res;
            using (var ms = new MemoryStream(bArr.Length * 6))
            {
                using (var stream = new ZipInputStream(new MemoryStream(bArr)))
                {
                    stream.GetNextEntry();
                    StreamUtil.TransferAll(stream, ms);
                }
                res = ms.ToArray();
            }
            return res;
        }

        public static byte[] GetFileNameFromZip(byte[] bArr, ref string fileName)
        {
            if (bArr == null)
                return null;
            byte[] res;
            using (var ms = new MemoryStream(bArr.Length * 6))
            {
                using (var stream = new ZipInputStream(new MemoryStream(bArr)))
                {
                    var entry = stream.GetNextEntry();
                    if (entry != null && entry.IsDirectory == false)
                    {
                        fileName = entry.Name;
                    }
                    StreamUtil.TransferAll(stream, ms);
                }
                res = ms.ToArray();
            }
            return res;
        }

        public static byte[] DoGZip(byte[] bArr)
        {
            if (bArr == null)
                return null;
            byte[] res;
            using (var ms = new MemoryStream(bArr.Length / 6))
            {
                using (var zos = new GZOutputStream(ms))
                {
                    zos.Write(bArr, 0, bArr.Length);
                }
                res = ms.ToArray();
            }
            return res;
        }

        public static byte[] DoUnGzip(byte[] bArr)
        {
            //Recommand C# implementation over C++ UnGzip 
            //byte[] gzipheader;
            //return DoUnGzip(bArr, out gzipheader);
            return DoUnGzip2(bArr);
        }

        public static byte[] DoUnGzip(byte[] bArr, out byte[] gzipHeader)
        {
            gzipHeader = null;
            if (bArr == null)
                return null;
            byte[] res;
            using (var ms = new MemoryStream(bArr.Length * 6))
            {
                using (var stream = new GZInputStream(new MemoryStream(bArr)))
                {
                    StreamUtil.TransferAll(stream, ms);
                    if (stream.Header != null)
                    {
                        gzipHeader = new byte[stream.Header.Length];
                        Array.Copy(stream.Header, gzipHeader, gzipHeader.Length);
                    }
                }
                res = ms.ToArray();
            }
            return res;
        }

        public static byte[] DoGZip2(byte[] bArr)
        {
            if (bArr == null)
                return null;
            byte[] res;
            using (var ms = new MemoryStream(bArr.Length / 6))
            {
                using (var compressedzipStream = new GZipStream(ms, CompressionMode.Compress, false))
                {
                    compressedzipStream.Write(bArr, 0, bArr.Length);
                }
                res = ms.ToArray();
            }
            return res;
        }

        public static byte[] DoGZip2Stream(Stream stream)
        {
            if (stream == null) return null;

            byte[] res;
            using (var ms = new MemoryStream())
            {
                using (var compressedzipStream = new GZipStream(ms, CompressionMode.Compress, false))
                {
                    stream.CopyTo(compressedzipStream);
                }
                res = ms.ToArray();
            }
            return res;
        }

        public static byte[] DoUnGzip2(byte[] bArr)
        {
            if (bArr == null)
                return null;
            byte[] res;
            using (var ms = new MemoryStream(bArr.Length * 6))
            {
                DoUnGzip(bArr, ms);
                res = ms.ToArray();
            }
            return res;
        }

        public static byte[] DoZipGZipStream(byte[] bArr, string fileName)
        {
            if (bArr == null)
                return null;

            using (var ms = new MemoryStream())
            {
                using (var stream = new GZipStream(new MemoryStream(bArr), CompressionMode.Decompress, false))
                {
                    byte[] res;
                    using (var zos = new ZipOutputStream(ms))
                    {
                        zos.PutNextEntry(new ZipEntry(fileName));
                        TransferAll(stream, zos);
                        zos.Finish();
                    }
                    res = ms.ToArray();
                    return res;
                }
            }
        }
        public static byte[] DoZipGZipStream(Stream gzipStream, string fileName)
        {
            if (gzipStream == null)
                return null;

            using (var ms = new MemoryStream())
            {
                using (var stream = new GZipStream(gzipStream, CompressionMode.Decompress, false))
                {
                    byte[] res;
                    using (var zos = new ZipOutputStream(ms))
                    {
                        zos.PutNextEntry(new ZipEntry(fileName));
                        TransferAll(stream, zos);
                        zos.Finish();
                    }
                    res = ms.ToArray();
                    return res;
                }
            }
        }

        public static void DoUnGzip(byte[] bArr, Stream outStream)
        {
            if (bArr == null)
                return;
            using (var stream = new GZipStream(new MemoryStream(bArr), CompressionMode.Decompress, false))
            {
                StreamUtil.TransferAll(stream, outStream);
            }
        }

        public static void AddFileToZip(string stZipPath, IEnumerable<string> filePaths)
        {
            FileStream fsZip = null;
            if (File.Exists(stZipPath))
                fsZip = new FileStream(stZipPath, FileMode.Open);
            else
                fsZip = File.Create(stZipPath);
            ZipOutputStream zipOutput = null;
            zipOutput = new ZipOutputStream(fsZip);
            zipOutput.SetLevel(6); // 0 - store only to 9 - means best compression

            foreach (string stFileName in filePaths)
            {
                FileStream fs = File.OpenRead(stFileName);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);

                ZipEntry entry = new ZipEntry(Path.GetFileName(stFileName));
                entry.DateTime = DateTime.Now;
                entry.Size = fs.Length;
                fs.Close();

                zipOutput.PutNextEntry(entry);
                zipOutput.Write(buffer, 0, buffer.Length);
            }

            zipOutput.Finish();
            zipOutput.Close();
            zipOutput = null;

        }

        public static byte[] ZipFiles(List<KeyValuePair<string, byte[]>> files)
        {
            var ms = new MemoryStream();
            ZipOutputStream zipOutput = new ZipOutputStream(ms);
            zipOutput.SetLevel(6); // 0 - store only to 9 - means best compression

            foreach (var kv in files)
            {
                byte[] buffer = kv.Value;
                ZipEntry entry = new ZipEntry(kv.Key);
                entry.DateTime = DateTime.Now;
                entry.Size = buffer.Length;

                zipOutput.PutNextEntry(entry);
                zipOutput.Write(buffer, 0, buffer.Length);
            }

            zipOutput.Finish();
            zipOutput.Close();
            zipOutput = null;

            return ms.ToArray();
        }

        public static byte[] ZipFilesNew(List<KeyValuePair<string, byte[]>> files)
        {

            using (var ms = new MemoryStream())
            {
                using (var zipArchi = new ZipArchive(ms, ZipArchiveMode.Create))
                {
                    foreach (var file in files)
                    {
                        byte[] bytes = file.Value;
                        var entry = zipArchi.CreateEntry(file.Key);
                        using (Stream stream = entry.Open())
                        {
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                return ms.ToArray();
            }

        }
    }
}